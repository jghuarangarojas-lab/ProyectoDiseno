using Iot.Application.Interfaces.Services;
using Iot.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Iot.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITelemetryService, TelemetryService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IAlertService, AlertService>();
        return services;
    }
}
