using System.Globalization;
using Restaurant.Domain;

namespace Restaurant.Web.Services;

/// <summary>
/// Exposes the active UI language. Derives from <see cref="CultureInfo.CurrentUICulture"/>,
/// which is set per-request by the localization middleware and — crucially — captured by the
/// Blazor Server circuit when it is established, so static SSR and interactive renders agree.
/// </summary>
public class LanguageState
{
    public Language Current => LanguageExtensions.FromCode(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    public bool IsRtl => Current.IsRtl();
    public string Dir => IsRtl ? "rtl" : "ltr";
    public string Code => Current.Code();

    /// <summary>Localized UI string for the current language.</summary>
    public string this[string key] => UiText.Get(key, Current);
}
