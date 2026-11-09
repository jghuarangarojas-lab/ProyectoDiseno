using Irrigation.Application.Interfaces.Messaging;
using Irrigation.Application.Interfaces.Repositories;
using Irrigation.Infrastructure.Messaging;
using Irrigation.Infrastructure.Persistence;
using Irrigation.Infrastructure.Persistence.Repositories;
using LurinSmart.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Irrigation.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));

        services.AddDatabaseFailover(configuration);

        services.AddDbContext<IrrigationDbContext>((provider, options) =>
            options.UseNpgsql(
                provider.GetRequiredService<DatabaseFailoverState>().ActiveConnection,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "irrigation")));

        services.AddScoped<IIrrigationRepository, IrrigationRepository>();
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        services.AddHostedService<MqttSubscriber>();

        return services;
    }
}
