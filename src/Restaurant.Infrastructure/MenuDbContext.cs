using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure;

public class MenuDbContext(DbContextOptions<MenuDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemImage> MenuItemImages => Set<MenuItemImage>();
    public DbSet<MenuItemTranslation> MenuItemTranslations => Set<MenuItemTranslation>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<BannerTranslation> BannerTranslations => Set<BannerTranslation>();
    public DbSet<ViewEvent> ViewEvents => Set<ViewEvent>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasMany(x => x.Children).WithOne(x => x.Parent!).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Translations).WithOne().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<CategoryTranslation>().HasIndex(x => new { x.CategoryId, x.Language }).IsUnique();

        b.Entity<MenuItem>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.HasOne(x => x.Category).WithMany(x => x.Items).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Images).WithOne().HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Translations).WithOne().HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<MenuItemTranslation>().HasIndex(x => new { x.MenuItemId, x.Language }).IsUnique();

        b.Entity<Banner>().HasMany(x => x.Translations).WithOne().HasForeignKey(x => x.BannerId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<ViewEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Type, x.CreatedUtc });
        });

        b.Entity<AdminUser>().HasIndex(x => x.Username).IsUnique();
    }
}
