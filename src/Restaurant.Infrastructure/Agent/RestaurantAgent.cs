using System.Text.RegularExpressions;
using Restaurant.Domain;
using Restaurant.Infrastructure.Services;

namespace Restaurant.Infrastructure.Agent;

/// <summary>
/// Natural-language admin agent. Turns instructions into a reviewable plan of
/// structured actions, then executes them. Destructive actions require confirmation.
/// </summary>
/// <remarks>
/// The intent parser here is a deterministic heuristic so the feature works with no
/// external dependency. It is intentionally behind the <see cref="IRestaurantAgent"/>
/// seam: a Claude-backed parser (claude-opus-4-8 with tool use) can replace
/// <see cref="PlanAsync"/> without touching callers or the executor.
/// </remarks>
public interface IRestaurantAgent
{
    Task<AgentPlan> PlanAsync(string instruction, Language lang, CancellationToken ct = default);
    Task<string> ExecuteAsync(AgentPlan plan, CancellationToken ct = default);
}

public enum AgentOp { CreateCategory, CreateItem, SetAvailability, AdjustPrices, Unknown }

public class AgentAction
{
    public AgentOp Op { get; set; }
    public string Summary { get; set; } = "";
    public bool IsDestructive { get; set; }
    public Dictionary<string, string> Args { get; set; } = new();
}

public class AgentPlan
{
    public string Instruction { get; set; } = "";
    public List<AgentAction> Actions { get; set; } = [];
    public string? Note { get; set; }
    public bool RequiresConfirmation => Actions.Any(a => a.IsDestructive);
    public bool HasActions => Actions.Count > 0;
}

public partial class RestaurantAgent(IMenuAdminService admin) : IRestaurantAgent
{
    public async Task<AgentPlan> PlanAsync(string instruction, Language lang, CancellationToken ct = default)
    {
        var plan = new AgentPlan { Instruction = instruction };
        var text = instruction.Trim();
        var lower = text.ToLowerInvariant();
        var cats = await admin.GetCategoriesFlatAsync(ct);

        // "Create a <name> category"
        var createCat = CreateCategoryRx().Match(text);
        if (createCat.Success)
        {
            var name = createCat.Groups["name"].Value.Trim();
            plan.Actions.Add(new AgentAction
            {
                Op = AgentOp.CreateCategory,
                Summary = $"Create category “{name}”",
                Args = { ["name"] = name },
            });
            return plan;
        }

        // "Add a new <name> for <price>₪ in the <category> category"
        var addItem = AddItemRx().Match(text);
        if (addItem.Success)
        {
            var name = addItem.Groups["name"].Value.Trim();
            var price = addItem.Groups["price"].Value;
            var catName = addItem.Groups["cat"].Value.Trim();
            var cat = MatchCategory(cats, catName);
            plan.Actions.Add(new AgentAction
            {
                Op = AgentOp.CreateItem,
                Summary = $"Add “{name}” at {price}₪ to {(cat is null ? catName : cat.T(Language.English).Name)}",
                Args = { ["name"] = name, ["price"] = price, ["categoryId"] = cat?.Id.ToString() ?? "", ["categoryName"] = catName },
            });
            if (cat is null) plan.Note = $"Category “{catName}” not found — it will be created automatically.";
            return plan;
        }

        // "Mark all <x> items as unavailable / available"
        var avail = AvailabilityRx().Match(text);
        if (avail.Success)
        {
            var available = !lower.Contains("unavailable") && !lower.Contains("out of stock") && !lower.Contains("hide");
            var subject = avail.Groups["subject"].Value.Trim();
            var cat = MatchCategory(cats, subject);
            plan.Actions.Add(new AgentAction
            {
                Op = AgentOp.SetAvailability,
                Summary = $"Mark {(string.IsNullOrEmpty(subject) ? "all" : subject)} items as {(available ? "available" : "unavailable")}",
                IsDestructive = true,
                Args =
                {
                    ["available"] = available.ToString(),
                    ["categoryId"] = cat?.Id.ToString() ?? "",
                    ["nameContains"] = cat is null ? subject : "",
                },
            });
            return plan;
        }

        // "Increase/decrease all <x> prices by <n>%"
        var price2 = PriceAdjustRx().Match(text);
        if (price2.Success)
        {
            var pct = decimal.Parse(price2.Groups["pct"].Value);
            if (lower.Contains("decrease") || lower.Contains("lower") || lower.Contains("reduce") || lower.Contains("discount"))
                pct = -pct;
            var subject = price2.Groups["subject"].Value.Trim();
            var cat = MatchCategory(cats, subject);
            plan.Actions.Add(new AgentAction
            {
                Op = AgentOp.AdjustPrices,
                Summary = $"{(pct >= 0 ? "Increase" : "Decrease")} {(cat is null ? "all" : cat.T(Language.English).Name)} prices by {Math.Abs(pct)}%",
                IsDestructive = true,
                Args = { ["percent"] = pct.ToString(), ["categoryId"] = cat?.Id.ToString() ?? "" },
            });
            return plan;
        }

        plan.Note = "I couldn't map that to an action yet. Try: “Add a new Mojito for 25₪ in the Drinks category”, " +
                    "“Mark all pizza items as unavailable”, “Create a Specials category”, or “Increase all drink prices by 5%”.";
        return plan;
    }

    public async Task<string> ExecuteAsync(AgentPlan plan, CancellationToken ct = default)
    {
        var results = new List<string>();
        foreach (var a in plan.Actions)
        {
            switch (a.Op)
            {
                case AgentOp.CreateCategory:
                {
                    var name = a.Args["name"];
                    await admin.UpsertCategoryAsync(NewCategory(name), ct);
                    results.Add($"Created category “{name}”.");
                    break;
                }
                case AgentOp.CreateItem:
                {
                    var name = a.Args["name"];
                    var price = decimal.TryParse(a.Args["price"], out var p) ? p : 0;
                    Guid catId;
                    if (Guid.TryParse(a.Args.GetValueOrDefault("categoryId"), out catId) is false || catId == Guid.Empty)
                    {
                        var cat = await admin.UpsertCategoryAsync(NewCategory(a.Args.GetValueOrDefault("categoryName", "Misc")), ct);
                        catId = cat.Id;
                    }
                    await admin.UpsertItemAsync(NewItem(name, price, catId), ct);
                    results.Add($"Added “{name}” at {price}₪.");
                    break;
                }
                case AgentOp.SetAvailability:
                {
                    var available = bool.Parse(a.Args["available"]);
                    Guid? catId = Guid.TryParse(a.Args.GetValueOrDefault("categoryId"), out var c) && c != Guid.Empty ? c : null;
                    var nameContains = a.Args.GetValueOrDefault("nameContains");
                    var n = await admin.SetAvailabilityAsync(available, catId, string.IsNullOrWhiteSpace(nameContains) ? null : nameContains, ct);
                    results.Add($"Updated availability on {n} item(s).");
                    break;
                }
                case AgentOp.AdjustPrices:
                {
                    var pct = decimal.Parse(a.Args["percent"]);
                    Guid? catId = Guid.TryParse(a.Args.GetValueOrDefault("categoryId"), out var c) && c != Guid.Empty ? c : null;
                    var n = await admin.AdjustPricesAsync(pct, catId, ct);
                    results.Add($"Adjusted prices on {n} item(s) by {pct}%.");
                    break;
                }
            }
        }
        return results.Count > 0 ? string.Join(" ", results) : "Nothing to do.";
    }

    static Category NewCategory(string name) => new()
    {
        Slug = Slugify(name),
        Icon = "🍴",
        Translations = [new() { Language = Language.English, Name = name }],
    };

    static MenuItem NewItem(string name, decimal price, Guid catId) => new()
    {
        Slug = Slugify(name) + "-" + Guid.NewGuid().ToString("N")[..6],
        CategoryId = catId,
        Price = price,
        IsAvailable = true,
        Translations = [new() { Language = Language.English, Name = name }],
    };

    static Category? MatchCategory(List<Category> cats, string subject)
    {
        if (string.IsNullOrWhiteSpace(subject)) return null;
        var s = subject.ToLowerInvariant().TrimEnd('s');
        return cats.FirstOrDefault(c =>
            c.Slug.Contains(s) ||
            c.Translations.Any(t => t.Name.ToLowerInvariant().Contains(s)));
    }

    static string Slugify(string s) =>
        SlugRx().Replace(s.ToLowerInvariant().Trim(), "-").Trim('-');

    [GeneratedRegex(@"create\s+(a\s+|an\s+)?(?<name>.+?)\s+category", RegexOptions.IgnoreCase)]
    private static partial Regex CreateCategoryRx();

    [GeneratedRegex(@"add\s+(a\s+|an\s+)?(new\s+)?(?<name>.+?)\s+for\s+(?<price>\d+(\.\d+)?)\s*₪?\s*(in|to)\s+the\s+(?<cat>.+?)\s+category", RegexOptions.IgnoreCase)]
    private static partial Regex AddItemRx();

    [GeneratedRegex(@"mark\s+(all\s+)?(?<subject>.*?)\s*items?\s+as\s+(un)?available|mark\s+(all\s+)?(?<subject>.*?)\s+as\s+(un)?available", RegexOptions.IgnoreCase)]
    private static partial Regex AvailabilityRx();

    [GeneratedRegex(@"(increase|decrease|raise|lower|reduce|discount)\s+(all\s+)?(?<subject>.*?)\s*prices?\s+by\s+(?<pct>\d+(\.\d+)?)\s*%", RegexOptions.IgnoreCase)]
    private static partial Regex PriceAdjustRx();

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex SlugRx();
}
