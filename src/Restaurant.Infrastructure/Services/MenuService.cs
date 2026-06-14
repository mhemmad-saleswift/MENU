using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure.Services;

public class MenuService(MenuDbContext db) : IMenuService
{
    public async Task<List<Category>> GetCategoryTreeAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var all = await db.Categories
            .Include(c => c.Translations)
            .Where(c => !activeOnly || c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        // Build the tree in memory from the flat list.
        var byParent = all.ToLookup(c => c.ParentId);
        foreach (var c in all)
            c.Children = byParent[c.Id].OrderBy(x => x.SortOrder).ToList();

        return byParent[null].OrderBy(c => c.SortOrder).ToList();
    }

    public Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Categories.Include(c => c.Translations).FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<List<MenuItem>> GetItemsAsync(Guid? categoryId = null, CancellationToken ct = default)
    {
        var q = db.MenuItems.Include(i => i.Images).Include(i => i.Translations).AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        return await q.OrderBy(i => i.SortOrder).ToListAsync(ct);
    }

    public Task<MenuItem?> GetItemBySlugAsync(string slug, CancellationToken ct = default) =>
        db.MenuItems.Include(i => i.Images).Include(i => i.Translations).Include(i => i.Category!).ThenInclude(c => c.Translations)
            .FirstOrDefaultAsync(i => i.Slug == slug, ct);

    public Task<List<MenuItem>> GetFeaturedAsync(int take = 8, CancellationToken ct = default) =>
        db.MenuItems.Include(i => i.Images).Include(i => i.Translations)
            .Where(i => i.IsRecommended || i.IsPopular)
            .OrderByDescending(i => i.IsRecommended).ThenByDescending(i => i.IsPopular)
            .Take(take).ToListAsync(ct);

    public Task<List<Banner>> GetActiveBannersAsync(CancellationToken ct = default) =>
        db.Banners.Include(x => x.Translations).Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(ct);

    public async Task<List<MenuItem>> SearchAsync(MenuSearchQuery query, CancellationToken ct = default)
    {
        var q = db.MenuItems.Include(i => i.Images).Include(i => i.Translations).AsQueryable();

        if (query.CategoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        if (query.MinPrice is { } min) q = q.Where(i => i.Price >= min);
        if (query.MaxPrice is { } max) q = q.Where(i => i.Price <= max);
        if (query.AvailableOnly) q = q.Where(i => i.IsAvailable);
        if (query.PopularOnly) q = q.Where(i => i.IsPopular || i.IsRecommended);
        if (query.RequiredTags != DietaryTag.None)
            q = q.Where(i => (i.DietaryTags & query.RequiredTags) == query.RequiredTags);

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var text = query.Text.Trim();
            q = q.Where(i => i.Translations.Any(t =>
                EF.Functions.Like(t.Name, $"%{text}%") ||
                (t.Description != null && EF.Functions.Like(t.Description, $"%{text}%"))));
        }

        return await q.OrderByDescending(i => i.IsPopular).ThenBy(i => i.SortOrder).Take(60).ToListAsync(ct);
    }
}
