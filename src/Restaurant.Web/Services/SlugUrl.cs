namespace Restaurant.Web.Services;

/// <summary>
/// Builds URL path segments from slugs. Slugs entered in admin may contain spaces, "&amp;" or
/// leading/trailing whitespace (browsers silently drop a trailing space from an href, which used
/// to break those category links), so trim and percent-encode before putting them in a link.
/// </summary>
public static class SlugUrl
{
    public static string Category(string slug) => "/menu/" + Uri.EscapeDataString(slug.Trim());
    public static string Item(string slug) => "/item/" + Uri.EscapeDataString(slug.Trim());
}
