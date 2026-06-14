using Restaurant.Domain;

namespace Restaurant.Web.Services;

/// <summary>Helpers to render enum flags as localized chips.</summary>
public static class Display
{
    static readonly (DietaryTag Tag, string Key, string Css)[] DietaryMap =
    [
        (DietaryTag.Vegan, "tag.vegan", "veg"),
        (DietaryTag.Vegetarian, "tag.vegetarian", "veg"),
        (DietaryTag.GlutenFree, "tag.glutenfree", "gf"),
        (DietaryTag.Spicy, "tag.spicy", "spicy"),
        (DietaryTag.Halal, "tag.halal", ""),
        (DietaryTag.DairyFree, "tag.dairyfree", ""),
        (DietaryTag.NutFree, "tag.nutfree", ""),
        (DietaryTag.Organic, "tag.organic", ""),
    ];

    public static IEnumerable<(string Label, string Css)> Tags(DietaryTag tags, Language lang) =>
        DietaryMap.Where(m => tags.HasFlag(m.Tag))
            .Select(m => (UiText.Get(m.Key, lang), m.Css));

    static readonly (Allergen A, string En, string Ar, string He)[] AllergenMap =
    [
        (Allergen.Gluten, "Gluten", "الغلوتين", "גלוטן"),
        (Allergen.Dairy, "Dairy", "الألبان", "חלב"),
        (Allergen.Eggs, "Eggs", "البيض", "ביצים"),
        (Allergen.Nuts, "Nuts", "المكسرات", "אגוזים"),
        (Allergen.Peanuts, "Peanuts", "الفول السوداني", "בוטנים"),
        (Allergen.Soy, "Soy", "الصويا", "סויה"),
        (Allergen.Fish, "Fish", "السمك", "דגים"),
        (Allergen.Shellfish, "Shellfish", "المحار", "פירות ים"),
        (Allergen.Sesame, "Sesame", "السمسم", "שומשום"),
    ];

    public static IEnumerable<string> Allergens(Allergen a, Language lang) =>
        AllergenMap.Where(m => a.HasFlag(m.A))
            .Select(m => lang switch { Language.Arabic => m.Ar, Language.Hebrew => m.He, _ => m.En });
}
