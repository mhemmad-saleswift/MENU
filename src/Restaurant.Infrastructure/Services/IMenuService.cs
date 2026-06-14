using Restaurant.Domain;

namespace Restaurant.Infrastructure.Services;

/// <summary>Customer-facing read operations over the menu.</summary>
public interface IMenuService
{
    Task<List<Category>> GetCategoryTreeAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<MenuItem>> GetItemsAsync(Guid? categoryId = null, CancellationToken ct = default);
    Task<MenuItem?> GetItemBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<MenuItem>> GetFeaturedAsync(int take = 8, CancellationToken ct = default);
    Task<List<Banner>> GetActiveBannersAsync(CancellationToken ct = default);
    Task<List<MenuItem>> SearchAsync(MenuSearchQuery query, CancellationToken ct = default);
}

public class MenuSearchQuery
{
    public string? Text { get; set; }
    public Language Language { get; set; } = Language.English;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DietaryTag RequiredTags { get; set; } = DietaryTag.None;
    public bool AvailableOnly { get; set; }
    public bool PopularOnly { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>Admin write operations. Also the surface the AI agent acts through.</summary>
public interface IMenuAdminService
{
    Task<List<Category>> GetCategoriesFlatAsync(CancellationToken ct = default);
    Task<Category> UpsertCategoryAsync(Category category, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);
    Task ReorderCategoriesAsync(IReadOnlyList<Guid> orderedIds, CancellationToken ct = default);

    Task<List<MenuItem>> GetItemsForAdminAsync(Guid? categoryId = null, CancellationToken ct = default);
    Task<MenuItem?> GetItemAsync(Guid id, CancellationToken ct = default);
    Task<MenuItem> UpsertItemAsync(MenuItem item, CancellationToken ct = default);
    Task DeleteItemAsync(Guid id, CancellationToken ct = default);

    /// <summary>Bulk set availability. Filter by category (null = all) and/or name contains.</summary>
    Task<int> SetAvailabilityAsync(bool available, Guid? categoryId = null, string? nameContains = null, CancellationToken ct = default);

    /// <summary>Adjust prices by a percentage. Positive raises, negative lowers.</summary>
    Task<int> AdjustPricesAsync(decimal percent, Guid? categoryId = null, CancellationToken ct = default);

    Task BulkDeleteItemsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
}

public interface IAnalyticsService
{
    Task RecordAsync(ViewEventType type, Guid? targetId, Language lang, string? term = null, CancellationToken ct = default);
    Task<AnalyticsSummary> GetSummaryAsync(int days = 30, CancellationToken ct = default);
}

public record AnalyticsSummary(
    int TotalViews,
    IReadOnlyList<RankedEntry> TopItems,
    IReadOnlyList<RankedEntry> TopCategories,
    IReadOnlyList<RankedEntry> TopSearches,
    IReadOnlyList<LanguageStat> Languages,
    IReadOnlyList<DailyCount> Daily);

public record RankedEntry(string Label, int Count, Guid? Id = null);
public record LanguageStat(Language Language, int Count);
public record DailyCount(DateOnly Day, int Count);
