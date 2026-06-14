using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Infrastructure.Agent;
using Restaurant.Infrastructure.Services;

namespace Restaurant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRestaurantInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Factory (not a scoped DbContext): in Blazor Server a scoped context lives for the whole
        // circuit, causing stale change-tracking + concurrency errors. Services create one per call.
        services.AddDbContextFactory<MenuDbContext>(o => o
            .UseSqlite(connectionString)
            .AddInterceptors(new SqlitePragmaInterceptor()));
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IMenuAdminService, MenuAdminService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IRestaurantAgent, RestaurantAgent>();
        return services;
    }
}
