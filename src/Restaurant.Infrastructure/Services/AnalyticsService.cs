using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure.Services;

public class AnalyticsService(MenuDbContext db) : IAnalyticsService
{
    public async Task RecordAsync(ViewEventType type, Guid? targetId, Language lang, string? term = null, CancellationToken ct = default)
    {
        db.ViewEvents.Add(new ViewEvent { Type = type, TargetId = targetId, Language = lang, Term = term });
        await db.SaveChangesAsync(ct);
    }

    public async Task<AnalyticsSummary> GetSummaryAsync(int days = 30, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var events = await db.ViewEvents.Where(e => e.CreatedUtc >= since).ToListAsync(ct);

        var itemNames = await db.MenuItemTranslations
            .Where(t => t.Language == Language.English)
            .ToDictionaryAsync(t => t.MenuItemId, t => t.Name, ct);
        var catNames = await db.CategoryTranslations
            .Where(t => t.Language == Language.English)
            .ToDictionaryAsync(t => t.CategoryId, t => t.Name, ct);

        var topItems = events.Where(e => e.Type == ViewEventType.ItemView && e.TargetId is not null)
            .GroupBy(e => e.TargetId!.Value)
            .Select(g => new RankedEntry(itemNames.GetValueOrDefault(g.Key, "—"), g.Count(), g.Key))
            .OrderByDescending(x => x.Count).Take(10).ToList();

        var topCats = events.Where(e => e.Type == ViewEventType.CategoryView && e.TargetId is not null)
            .GroupBy(e => e.TargetId!.Value)
            .Select(g => new RankedEntry(catNames.GetValueOrDefault(g.Key, "—"), g.Count(), g.Key))
            .OrderByDescending(x => x.Count).Take(10).ToList();

        var topSearches = events.Where(e => e.Type == ViewEventType.Search && !string.IsNullOrWhiteSpace(e.Term))
            .GroupBy(e => e.Term!.Trim().ToLowerInvariant())
            .Select(g => new RankedEntry(g.Key, g.Count()))
            .OrderByDescending(x => x.Count).Take(10).ToList();

        var langs = events.GroupBy(e => e.Language)
            .Select(g => new LanguageStat(g.Key, g.Count()))
            .OrderByDescending(x => x.Count).ToList();

        var daily = events.GroupBy(e => DateOnly.FromDateTime(e.CreatedUtc))
            .Select(g => new DailyCount(g.Key, g.Count()))
            .OrderBy(x => x.Day).ToList();

        return new AnalyticsSummary(events.Count, topItems, topCats, topSearches, langs, daily);
    }
}
