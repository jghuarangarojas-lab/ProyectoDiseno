using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsInventory.Application.Abstractions;
using MsInventory.Domain.Abstractions;
using MsInventory.Infrastructure.Hubs;
using MsInventory.Infrastructure.Notifications;
using MsInventory.Infrastructure.Persistence;
using MsInventory.Infrastructure.Persistence.Repositories;
using MsInventory.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace MsInventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalFileStorageOptions>(
            configuration.GetSection(LocalFileStorageOptions.SectionName));

        services.AddDbContext<InventoryDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("InventoryDb"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<ILowStockNotifier, LowStockNotifier>();

        return services;
    }
}
