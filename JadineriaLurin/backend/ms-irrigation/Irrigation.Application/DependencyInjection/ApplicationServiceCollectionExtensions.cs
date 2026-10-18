using Irrigation.Application.Interfaces.Services;
using Irrigation.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Irrigation.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIrrigationService, IrrigationService>();
        return services;
    }
}
