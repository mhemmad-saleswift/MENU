using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Infrastructure.Agent;
using Restaurant.Infrastructure.Services;

namespace Restaurant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRestaurantInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MenuDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IMenuAdminService, MenuAdminService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IRestaurantAgent, RestaurantAgent>();
        return services;
    }
}
