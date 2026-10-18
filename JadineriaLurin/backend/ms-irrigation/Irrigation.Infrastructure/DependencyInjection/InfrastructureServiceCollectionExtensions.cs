using Irrigation.Application.Interfaces.Messaging;
using Irrigation.Application.Interfaces.Repositories;
using Irrigation.Infrastructure.Messaging;
using Irrigation.Infrastructure.Persistence;
using Irrigation.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Irrigation.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));

        services.AddDbContext<IrrigationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__irrigation_migrations", "public")));

        services.AddScoped<IIrrigationRepository, IrrigationRepository>();
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        services.AddHostedService<MqttSubscriber>();

        return services;
    }
}
