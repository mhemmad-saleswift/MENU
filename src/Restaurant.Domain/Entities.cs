namespace Restaurant.Domain;

/// <summary>A menu category. Supports unlimited nesting via <see cref="ParentId"/>.</summary>
public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = "";
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];

    public List<CategoryTranslation> Translations { get; set; } = [];
    public List<MenuItem> Items { get; set; } = [];

    public CategoryTranslation T(Language lang) =>
        Translations.FirstOrDefault(t => t.Language == lang)
        ?? Translations.FirstOrDefault(t => t.Language == Language.English)
        ?? Translations.FirstOrDefault()
        ?? new CategoryTranslation { Name = Slug };
}

public class CategoryTranslation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public Language Language { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

/// <summary>A single menu item belonging to a category.</summary>
public class MenuItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = "";
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal Price { get; set; }
    public string Currency { get; set; } = "₪";

    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; }
    public bool IsRecommended { get; set; }

    public int SortOrder { get; set; }

    public DietaryTag DietaryTags { get; set; } = DietaryTag.None;
    public Allergen Allergens { get; set; } = Allergen.None;

    /// <summary>URL of an optional showcase video.</summary>
    public string? VideoUrl { get; set; }

    /// <summary>URL of a glTF/GLB 3D model for AR viewing. When set, AR is offered.</summary>
    public string? Model3dUrl { get; set; }

    public List<MenuItemImage> Images { get; set; } = [];
    public List<MenuItemTranslation> Translations { get; set; } = [];

    public bool HasAr => !string.IsNullOrWhiteSpace(Model3dUrl);
    public string? PrimaryImage => Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).FirstOrDefault()?.Url;

    public MenuItemTranslation T(Language lang) =>
        Translations.FirstOrDefault(t => t.Language == lang)
        ?? Translations.FirstOrDefault(t => t.Language == Language.English)
        ?? Translations.FirstOrDefault()
        ?? new MenuItemTranslation { Name = Slug };
}

public class MenuItemImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MenuItemId { get; set; }
    public string Url { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class MenuItemTranslation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MenuItemId { get; set; }
    public Language Language { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    /// <summary>Comma-separated, localized ingredient list.</summary>
    public string? Ingredients { get; set; }
}

/// <summary>Promotional banner shown on the customer home/menu.</summary>
public class Banner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ImageUrl { get; set; } = "";
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BannerTranslation> Translations { get; set; } = [];

    public BannerTranslation T(Language lang) =>
        Translations.FirstOrDefault(t => t.Language == lang)
        ?? Translations.FirstOrDefault(t => t.Language == Language.English)
        ?? Translations.FirstOrDefault()
        ?? new BannerTranslation();
}

public class BannerTranslation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BannerId { get; set; }
    public Language Language { get; set; }
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
}

/// <summary>Lightweight analytics event for views and searches.</summary>
public class ViewEvent
{
    public long Id { get; set; }
    public ViewEventType Type { get; set; }
    public Guid? TargetId { get; set; }
    public string? Term { get; set; }
    public Language Language { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Admin user for the dashboard.</summary>
public class AdminUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
