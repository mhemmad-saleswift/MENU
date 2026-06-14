using System.Globalization;
using System.IO.Compression;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Restaurant.Domain;
using Restaurant.Infrastructure;
using Restaurant.Infrastructure.Services;
using Restaurant.Web.Components;
using Restaurant.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Persistent data location. On Azure App Service the deployment folder (wwwroot) is replaced on
// every publish, so the SQLite database and uploaded images must live under the persistent
// %HOME%/data (e.g. /home/data). Locally this falls back to the project folder.
var dataPath = builder.Configuration["DataPath"];
if (string.IsNullOrWhiteSpace(dataPath))
{
    var home = Environment.GetEnvironmentVariable("HOME");
    dataPath = !builder.Environment.IsDevelopment() && !string.IsNullOrEmpty(home)
        ? Path.Combine(home, "data")
        : builder.Environment.ContentRootPath;
}
Directory.CreateDirectory(dataPath);
var uploadsPath = Path.Combine(dataPath, "uploads");
Directory.CreateDirectory(uploadsPath);
builder.Services.AddSingleton(new StoragePaths(dataPath, uploadsPath));

// Data + business services.
var dbPath = Path.Combine(dataPath, "restaurant.db");
builder.Services.AddRestaurantInfrastructure($"Data Source={dbPath}");

// Per-request UI language.
builder.Services.AddScoped<LanguageState>();
builder.Services.AddHttpContextAccessor();

// Uploaded product image storage.
builder.Services.AddScoped<IImageStorage, LocalImageStorage>();

// Cookie auth for the admin area.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/admin/login";
        o.AccessDeniedPath = "/admin/login";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Migrate (preserving any existing data), seed content, and provision the admin account.
{
    var dbf = app.Services.GetRequiredService<IDbContextFactory<MenuDbContext>>();
    await using var db = await dbf.CreateDbContextAsync();
    await DbInitializer.InitAsync(db);
    await DbSeeder.SeedAsync(db);

    var adminUser = builder.Configuration["Admin:Username"] ?? "admin";
    var adminPass = builder.Configuration["Admin:Password"];
    if (string.IsNullOrWhiteSpace(adminPass))
    {
        if (app.Environment.IsDevelopment())
            adminPass = "admin123"; // dev convenience only
        else
            throw new InvalidOperationException(
                "Admin password is not configured. Set the 'Admin__Password' environment variable before starting in Production.");
    }
    await DbInitializer.EnsureAdminAsync(db, adminUser, adminPass);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Serve uploaded product images from the persistent data folder at /uploads.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
});

app.UseAuthentication();
app.UseAuthorization();

// Resolve culture from the "lang" cookie. Setting CurrentUICulture here means the value is
// also captured by the Blazor Server circuit established during this request.
var supportedCultures = new[] { "en", "ar", "he" };
var locOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
locOptions.RequestCultureProviders.Clear();
locOptions.RequestCultureProviders.Add(new CustomRequestCultureProvider(ctx =>
{
    var code = LanguageExtensions.FromCode(ctx.Request.Cookies["lang"]).Code();
    return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(code));
}));
app.UseRequestLocalization(locOptions);

// First-visit gate: send customers without a chosen language to the welcome screen.
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "/";
    var isCustomerPage = path == "/" || path.StartsWith("/menu") || path.StartsWith("/item");
    if (HttpMethods.IsGet(ctx.Request.Method)
        && isCustomerPage
        && !ctx.Request.Cookies.ContainsKey("lang"))
    {
        var ret = Uri.EscapeDataString(path + ctx.Request.QueryString);
        ctx.Response.Redirect($"/welcome?returnUrl={ret}");
        return;
    }
    await next();
});

app.UseAntiforgery();

// ---- Endpoints ----
app.MapGet("/set-language/{code}", (string code, string? returnUrl, HttpContext ctx) =>
{
    ctx.Response.Cookies.Append("lang", LanguageExtensions.FromCode(code).Code(),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
    return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
});

app.MapGet("/set-theme/{theme}", (string theme, string? returnUrl, HttpContext ctx) =>
{
    ctx.Response.Cookies.Append("theme", theme == "dark" ? "dark" : "light",
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
    return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
});

app.MapPost("/admin/login", async (HttpContext ctx, IDbContextFactory<MenuDbContext> dbf,
    [FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl) =>
{
    await using var db = await dbf.CreateDbContextAsync();
    var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username);
    if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        return Results.Redirect("/admin/login?error=1");

    var claims = new List<Claim> { new(ClaimTypes.Name, user.DisplayName), new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/admin" : returnUrl);
}).DisableAntiforgery();

app.MapPost("/admin/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// Admin backup: a ZIP containing a consistent snapshot of the SQLite database + all uploaded images.
app.MapGet("/admin/backup", async (IDbContextFactory<MenuDbContext> dbf, StoragePaths paths) =>
{
    var ms = new MemoryStream();
    using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
    {
        // Consistent DB snapshot via SQLite's online backup API (safe while the app runs).
        var tempDb = Path.Combine(Path.GetTempPath(), $"viva-backup-{Guid.NewGuid():N}.db");
        await using (var db = await dbf.CreateDbContextAsync())
        {
            var src = (SqliteConnection)db.Database.GetDbConnection();
            await src.OpenAsync();
            // Pooling=False so the temp file's handle is released on dispose (otherwise the
            // pooled connection keeps it locked and the zip step fails).
            await using var dst = new SqliteConnection($"Data Source={tempDb};Pooling=False");
            await dst.OpenAsync();
            src.BackupDatabase(dst);
        }
        zip.CreateEntryFromFile(tempDb, "restaurant.db");
        File.Delete(tempDb);

        if (Directory.Exists(paths.UploadsPath))
            foreach (var file in Directory.GetFiles(paths.UploadsPath))
                zip.CreateEntryFromFile(file, $"uploads/{Path.GetFileName(file)}");
    }
    ms.Position = 0;
    var name = $"viva-italia-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
    return Results.File(ms, "application/zip", name);
}).RequireAuthorization();

// ---- API-first read endpoints ----
var api = app.MapGroup("/api/menu");
api.MapGet("/categories", async (IMenuService menu) =>
    Results.Ok(await menu.GetCategoryTreeAsync()));
api.MapGet("/items", async (IMenuService menu, Guid? categoryId) =>
    Results.Ok(await menu.GetItemsAsync(categoryId)));
api.MapGet("/items/{slug}", async (IMenuService menu, string slug) =>
    await menu.GetItemBySlugAsync(slug) is { } item ? Results.Ok(item) : Results.NotFound());

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
