using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure.Services;

// Uses a short-lived DbContext per operation (via the factory). In Blazor Server a scoped
// DbContext lives for the whole circuit, which causes stale change-tracking and the
// "expected to affect 1 row but affected 0" concurrency errors on repeated saves.
public class MenuAdminService(IDbContextFactory<MenuDbContext> dbf) : IMenuAdminService
{
    public async Task<List<Category>> GetCategoriesFlatAsync(CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        return await db.Categories.Include(c => c.Translations).OrderBy(c => c.SortOrder).ToListAsync(ct);
    }

    public async Task<Category> UpsertCategoryAsync(Category category, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);

        if (!await db.Categories.AnyAsync(c => c.Id == category.Id, ct))
        {
            if (category.SortOrder == 0)
                category.SortOrder = (await db.Categories.MaxAsync(c => (int?)c.SortOrder, ct) ?? 0) + 1;
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
            return category;
        }

        // Update the row's columns directly (no change-tracking → no concurrency check).
        await db.Categories.Where(c => c.Id == category.Id).ExecuteUpdateAsync(s => s
            .SetProperty(c => c.Slug, category.Slug)
            .SetProperty(c => c.Icon, category.Icon)
            .SetProperty(c => c.IsActive, category.IsActive)
            .SetProperty(c => c.ParentId, category.ParentId)
            .SetProperty(c => c.SortOrder, category.SortOrder), ct);

        // Replace its translations.
        await db.CategoryTranslations.Where(t => t.CategoryId == category.Id).ExecuteDeleteAsync(ct);
        foreach (var tr in category.Translations) { tr.Id = Guid.NewGuid(); tr.CategoryId = category.Id; }
        db.CategoryTranslations.AddRange(category.Translations);
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        await db.Categories.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task ReorderCategoriesAsync(IReadOnlyList<Guid> orderedIds, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        var cats = await db.Categories.Where(c => orderedIds.Contains(c.Id)).ToListAsync(ct);
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var c = cats.FirstOrDefault(x => x.Id == orderedIds[i]);
            if (c is not null) c.SortOrder = i;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<MenuItem>> GetItemsForAdminAsync(Guid? categoryId = null, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        var q = db.MenuItems.Include(i => i.Images).Include(i => i.Translations).AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        return await q.OrderBy(i => i.SortOrder).ToListAsync(ct);
    }

    public async Task<MenuItem?> GetItemAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        return await db.MenuItems.Include(i => i.Images).Include(i => i.Translations)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<MenuItem> UpsertItemAsync(MenuItem item, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);

        if (!await db.MenuItems.AnyAsync(i => i.Id == item.Id, ct))
        {
            db.MenuItems.Add(item);
            await db.SaveChangesAsync(ct);
            return item;
        }

        // Update the row's columns directly (no change-tracking → no concurrency check).
        await db.MenuItems.Where(i => i.Id == item.Id).ExecuteUpdateAsync(s => s
            .SetProperty(i => i.Slug, item.Slug)
            .SetProperty(i => i.CategoryId, item.CategoryId)
            .SetProperty(i => i.Price, item.Price)
            .SetProperty(i => i.Currency, item.Currency)
            .SetProperty(i => i.IsAvailable, item.IsAvailable)
            .SetProperty(i => i.IsPopular, item.IsPopular)
            .SetProperty(i => i.IsRecommended, item.IsRecommended)
            .SetProperty(i => i.SortOrder, item.SortOrder)
            .SetProperty(i => i.DietaryTags, item.DietaryTags)
            .SetProperty(i => i.Allergens, item.Allergens)
            .SetProperty(i => i.VideoUrl, item.VideoUrl)
            .SetProperty(i => i.Model3dUrl, item.Model3dUrl), ct);

        // Replace its images + translations.
        await db.MenuItemImages.Where(x => x.MenuItemId == item.Id).ExecuteDeleteAsync(ct);
        await db.MenuItemTranslations.Where(x => x.MenuItemId == item.Id).ExecuteDeleteAsync(ct);

        foreach (var img in item.Images) { img.Id = Guid.NewGuid(); img.MenuItemId = item.Id; }
        foreach (var tr in item.Translations) { tr.Id = Guid.NewGuid(); tr.MenuItemId = item.Id; }
        db.MenuItemImages.AddRange(item.Images);
        db.MenuItemTranslations.AddRange(item.Translations);
        await db.SaveChangesAsync(ct);
        return item;
    }

    public async Task DeleteItemAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        await db.MenuItems.Where(i => i.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task<int> SetAvailabilityAsync(bool available, Guid? categoryId = null, string? nameContains = null, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        var q = db.MenuItems.AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        if (!string.IsNullOrWhiteSpace(nameContains))
            q = q.Where(i => i.Translations.Any(t => EF.Functions.Like(t.Name, $"%{nameContains}%")));
        return await q.ExecuteUpdateAsync(s => s.SetProperty(i => i.IsAvailable, available), ct);
    }

    public async Task<int> AdjustPricesAsync(decimal percent, Guid? categoryId = null, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        var q = db.MenuItems.AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        var factor = 1m + percent / 100m;
        return await q.ExecuteUpdateAsync(s => s.SetProperty(i => i.Price, i => Math.Round(i.Price * factor, 2)), ct);
    }

    public async Task BulkDeleteItemsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        await db.MenuItems.Where(i => ids.Contains(i.Id)).ExecuteDeleteAsync(ct);
    }

    public async Task<Banner> GetHomeBannerAsync(CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        return await db.Banners.Include(b => b.Translations).OrderBy(b => b.SortOrder).FirstOrDefaultAsync(ct)
            ?? new Banner { IsActive = true, SortOrder = 1 };
    }

    public async Task UpsertBannerAsync(Banner banner, CancellationToken ct = default)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);

        if (!await db.Banners.AnyAsync(b => b.Id == banner.Id, ct))
        {
            db.Banners.Add(banner);
            await db.SaveChangesAsync(ct);
            return;
        }

        await db.Banners.Where(b => b.Id == banner.Id).ExecuteUpdateAsync(s => s
            .SetProperty(b => b.ImageUrl, banner.ImageUrl)
            .SetProperty(b => b.LinkUrl, banner.LinkUrl)
            .SetProperty(b => b.IsActive, banner.IsActive)
            .SetProperty(b => b.SortOrder, banner.SortOrder), ct);

        await db.BannerTranslations.Where(t => t.BannerId == banner.Id).ExecuteDeleteAsync(ct);
        foreach (var tr in banner.Translations) { tr.Id = Guid.NewGuid(); tr.BannerId = banner.Id; }
        db.BannerTranslations.AddRange(banner.Translations);
        await db.SaveChangesAsync(ct);
    }
}
