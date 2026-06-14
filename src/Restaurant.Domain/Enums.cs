namespace Restaurant.Domain;

/// <summary>Supported content languages. English is LTR; Arabic and Hebrew are RTL.</summary>
public enum Language
{
    English = 0,
    Arabic = 1,
    Hebrew = 2,
}

public static class LanguageExtensions
{
    public static string Code(this Language lang) => lang switch
    {
        Language.Arabic => "ar",
        Language.Hebrew => "he",
        _ => "en",
    };

    public static bool IsRtl(this Language lang) => lang is Language.Arabic or Language.Hebrew;

    public static string NativeName(this Language lang) => lang switch
    {
        Language.Arabic => "العربية",
        Language.Hebrew => "עברית",
        _ => "English",
    };

    public static Language FromCode(string? code) => code?.ToLowerInvariant() switch
    {
        "ar" => Language.Arabic,
        "he" => Language.Hebrew,
        _ => Language.English,
    };
}

/// <summary>Dietary classification tags shown as badges on items.</summary>
[Flags]
public enum DietaryTag
{
    None = 0,
    Vegan = 1 << 0,
    Vegetarian = 1 << 1,
    GlutenFree = 1 << 2,
    Spicy = 1 << 3,
    Halal = 1 << 4,
    DairyFree = 1 << 5,
    NutFree = 1 << 6,
    Organic = 1 << 7,
}

/// <summary>Common allergens declared per item.</summary>
[Flags]
public enum Allergen
{
    None = 0,
    Gluten = 1 << 0,
    Dairy = 1 << 1,
    Eggs = 1 << 2,
    Nuts = 1 << 3,
    Peanuts = 1 << 4,
    Soy = 1 << 5,
    Fish = 1 << 6,
    Shellfish = 1 << 7,
    Sesame = 1 << 8,
}

/// <summary>Type of tracked analytics event.</summary>
public enum ViewEventType
{
    CategoryView = 0,
    ItemView = 1,
    Search = 2,
}
