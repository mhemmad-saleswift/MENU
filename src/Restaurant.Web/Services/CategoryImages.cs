using Restaurant.Domain;

namespace Restaurant.Web.Services;

/// <summary>Picks a representative product photo for each category (used on Home and /menu cards).</summary>
public static class CategoryImages
{
    /// <summary>Chef's pick first, then a popular dish, then any dish with a photo; null when none has one.</summary>
    public static Dictionary<Guid, string?> Pick(IEnumerable<Category> categories, IReadOnlyCollection<MenuItem> items)
    {
        var map = new Dictionary<Guid, string?>();
        foreach (var c in categories)
        {
            var its = items.Where(i => i.CategoryId == c.Id).ToList();
            map[c.Id] =
                its.FirstOrDefault(i => i.IsRecommended && i.PrimaryImage != null)?.PrimaryImage
                ?? its.FirstOrDefault(i => i.IsPopular && i.PrimaryImage != null)?.PrimaryImage
                ?? its.FirstOrDefault(i => i.PrimaryImage != null)?.PrimaryImage;
        }
        return map;
    }
}
