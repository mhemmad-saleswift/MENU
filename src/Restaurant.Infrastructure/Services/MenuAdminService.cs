using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure.Services;

public class MenuAdminService(MenuDbContext db) : IMenuAdminService
{
    public Task<List<Category>> GetCategoriesFlatAsync(CancellationToken ct = default) =>
        db.Categories.Include(c => c.Translations).OrderBy(c => c.SortOrder).ToListAsync(ct);

    public async Task<Category> UpsertCategoryAsync(Category category, CancellationToken ct = default)
    {
        var existing = await db.Categories.Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == category.Id, ct);

        if (existing is null)
        {
            if (category.SortOrder == 0)
                category.SortOrder = (await db.Categories.MaxAsync(c => (int?)c.SortOrder, ct) ?? 0) + 1;
            db.Categories.Add(category);
        }
        else
        {
            existing.Slug = category.Slug;
            existing.Icon = category.Icon;
            existing.IsActive = category.IsActive;
            existing.ParentId = category.ParentId;
            existing.SortOrder = category.SortOrder;
            // Replace translations.
            db.CategoryTranslations.RemoveRange(existing.Translations);
            existing.Translations = category.Translations;
            category = existing;
        }
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var c = await db.Categories.FindAsync([id], ct);
        if (c is not null) { db.Categories.Remove(c); await db.SaveChangesAsync(ct); }
    }

    public async Task ReorderCategoriesAsync(IReadOnlyList<Guid> orderedIds, CancellationToken ct = default)
    {
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
        var q = db.MenuItems.Include(i => i.Images).Include(i => i.Translations).AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        return await q.OrderBy(i => i.SortOrder).ToListAsync(ct);
    }

    public Task<MenuItem?> GetItemAsync(Guid id, CancellationToken ct = default) =>
        db.MenuItems.Include(i => i.Images).Include(i => i.Translations).FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<MenuItem> UpsertItemAsync(MenuItem item, CancellationToken ct = default)
    {
        var existing = await db.MenuItems.Include(i => i.Images).Include(i => i.Translations)
            .FirstOrDefaultAsync(i => i.Id == item.Id, ct);

        if (existing is null)
        {
            db.MenuItems.Add(item);
        }
        else
        {
            existing.Slug = item.Slug;
            existing.CategoryId = item.CategoryId;
            existing.Price = item.Price;
            existing.Currency = item.Currency;
            existing.IsAvailable = item.IsAvailable;
            existing.IsPopular = item.IsPopular;
            existing.IsRecommended = item.IsRecommended;
            existing.SortOrder = item.SortOrder;
            existing.DietaryTags = item.DietaryTags;
            existing.Allergens = item.Allergens;
            existing.VideoUrl = item.VideoUrl;
            existing.Model3dUrl = item.Model3dUrl;
            db.MenuItemImages.RemoveRange(existing.Images);
            db.MenuItemTranslations.RemoveRange(existing.Translations);
            existing.Images = item.Images;
            existing.Translations = item.Translations;
            item = existing;
        }
        await db.SaveChangesAsync(ct);
        return item;
    }

    public async Task DeleteItemAsync(Guid id, CancellationToken ct = default)
    {
        var i = await db.MenuItems.FindAsync([id], ct);
        if (i is not null) { db.MenuItems.Remove(i); await db.SaveChangesAsync(ct); }
    }

    public async Task<int> SetAvailabilityAsync(bool available, Guid? categoryId = null, string? nameContains = null, CancellationToken ct = default)
    {
        var q = db.MenuItems.AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        if (!string.IsNullOrWhiteSpace(nameContains))
            q = q.Where(i => i.Translations.Any(t => EF.Functions.Like(t.Name, $"%{nameContains}%")));
        return await q.ExecuteUpdateAsync(s => s.SetProperty(i => i.IsAvailable, available), ct);
    }

    public async Task<int> AdjustPricesAsync(decimal percent, Guid? categoryId = null, CancellationToken ct = default)
    {
        var q = db.MenuItems.AsQueryable();
        if (categoryId is { } cid) q = q.Where(i => i.CategoryId == cid);
        var factor = 1m + percent / 100m;
        return await q.ExecuteUpdateAsync(s => s.SetProperty(i => i.Price, i => Math.Round(i.Price * factor, 2)), ct);
    }

    public async Task BulkDeleteItemsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        await db.MenuItems.Where(i => ids.Contains(i.Id)).ExecuteDeleteAsync(ct);
    }
}
