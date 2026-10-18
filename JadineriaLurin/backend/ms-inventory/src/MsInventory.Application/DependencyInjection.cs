using Microsoft.Extensions.DependencyInjection;
using MsInventory.Application.Interfaces;
using MsInventory.Application.Services;

namespace MsInventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
        return services;
    }
}
