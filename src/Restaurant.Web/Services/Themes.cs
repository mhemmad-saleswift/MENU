namespace Restaurant.Web.Services;

/// <summary>Customer-facing visual themes, selected via the "theme" cookie.</summary>
public static class Themes
{
    public const string Dark = "dark";
    public const string Light = "light";
    public const string Classic = "classic";
    public const string Royal = "royal";

    /// <summary>All themes in picker order: (id, icon).</summary>
    public static readonly (string Id, string Icon)[] All =
    [
        (Dark, "🌙"),
        (Light, "☀️"),
        (Classic, "🍷"),
        (Royal, "⚜️"),
    ];

    /// <summary>The theme visitors get until they pick one.</summary>
    public const string Default = Royal;

    /// <summary>Maps any cookie/route value to a known theme id; missing or unknown values fall back to <see cref="Default"/>.</summary>
    public static string Normalize(string? value) => value switch
    {
        Dark => Dark,
        Light => Light,
        Classic => Classic,
        Royal => Royal,
        _ => Default,
    };

    public static string Icon(string theme) => Array.Find(All, t => t.Id == theme).Icon ?? "🌙";
}
