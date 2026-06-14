using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Restaurant.Infrastructure;

/// <summary>
/// Design-time factory used by `dotnet ef`. Building the context here (instead of via the
/// web host) keeps migration commands from running the app's startup/seed code.
/// The connection string is irrelevant for scaffolding — only the model shape is read.
/// </summary>
public class MenuDbContextFactory : IDesignTimeDbContextFactory<MenuDbContext>
{
    public MenuDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlite("Data Source=restaurant.db")
            .Options;
        return new MenuDbContext(options);
    }
}
