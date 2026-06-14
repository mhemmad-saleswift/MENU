using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure;

/// <summary>
/// Schema + admin bootstrap. Brings the database up to the latest EF migration while
/// preserving data from any legacy database that was created with <c>EnsureCreated()</c>.
/// </summary>
public static class DbInitializer
{
    public static async Task InitAsync(MenuDbContext db)
    {
        var hasHistory = await TableExistsAsync(db, "__EFMigrationsHistory");
        var hasLegacyTables = await TableExistsAsync(db, "Categories");

        if (hasLegacyTables && !hasHistory)
        {
            // The database was created by EnsureCreated() (no migration history). Baseline it:
            // record the initial migration as already applied so Migrate() won't try to
            // recreate the existing tables — keeping all current categories and products.
            var initial = db.Database.GetMigrations().First();
            await db.Database.ExecuteSqlRawAsync(
                """CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" ("MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY, "ProductVersion" TEXT NOT NULL);""");
            await db.Database.ExecuteSqlRawAsync(
                """INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ({0}, {1});""",
                initial, "10.0.0");
        }

        // Applies pending migrations; on a brand-new database this creates the full schema.
        await db.Database.MigrateAsync();
    }

    /// <summary>Creates or rotates the admin account from configuration (idempotent).</summary>
    public static async Task EnsureAdminAsync(MenuDbContext db, string username, string password, string displayName = "Administrator")
    {
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username);

        // Adopt a pre-existing lone admin (e.g. a legacy "admin") under the configured username.
        if (user is null)
        {
            var all = await db.AdminUsers.ToListAsync();
            if (all.Count == 1) { user = all[0]; user.Username = username; }
        }

        if (user is null)
            db.AdminUsers.Add(new AdminUser { Username = username, DisplayName = displayName, PasswordHash = PasswordHasher.Hash(password) });
        else
            user.PasswordHash = PasswordHasher.Hash(password);

        await db.SaveChangesAsync();
    }

    static async Task<bool> TableExistsAsync(MenuDbContext db, string name)
    {
        var count = await db.Database
            .SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM sqlite_master WHERE type='table' AND name = {0}", name)
            .FirstAsync();
        return count > 0;
    }
}
