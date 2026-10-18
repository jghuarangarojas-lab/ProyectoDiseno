using Iot.Application.Interfaces.External;
using Iot.Application.Interfaces.Repositories;
using Iot.Infrastructure.External;
using Iot.Infrastructure.Messaging;
using Iot.Infrastructure.Persistence;
using Iot.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iot.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));
        services.Configure<IrrigationApiOptions>(configuration.GetSection(IrrigationApiOptions.SectionName));

        services.AddDbContext<IotDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__iot_migrations", "public")));

        services.AddScoped<IIotDeviceRepository, IotDeviceRepository>();
        services.AddScoped<ITelemetryRepository, TelemetryRepository>();
        services.AddScoped<IDeviceAlertRepository, DeviceAlertRepository>();

        var irrigationOptions = configuration.GetSection(IrrigationApiOptions.SectionName).Get<IrrigationApiOptions>()
            ?? new IrrigationApiOptions();

        services.AddHttpClient<IIrrigationClient, IrrigationClient>(client =>
        {
            client.BaseAddress = new Uri($"{irrigationOptions.BaseUrl.TrimEnd('/')}/");
            client.Timeout = TimeSpan.FromSeconds(irrigationOptions.TimeoutSeconds);
        });

        services.AddHostedService<MqttTelemetryWorker>();

        return services;
    }
}
