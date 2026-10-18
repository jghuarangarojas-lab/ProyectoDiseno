using Microsoft.Extensions.DependencyInjection;
using MsDelivery.Application.Interfaces;
using MsDelivery.Application.Services;

namespace MsDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDeliveryService, DeliveryService>();
        return services;
    }
}
