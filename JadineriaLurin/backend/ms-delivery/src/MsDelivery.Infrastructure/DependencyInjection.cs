using LurinSmart.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsDelivery.Application.Abstractions;
using MsDelivery.Domain.Abstractions;
using MsDelivery.Infrastructure.Persistence;
using MsDelivery.Infrastructure.Persistence.Repositories;
using MsDelivery.Infrastructure.Storage;
using MsDelivery.Infrastructure.WhatsApp;

namespace MsDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalFileStorageOptions>(
            configuration.GetSection(LocalFileStorageOptions.SectionName));

        services.Configure<WhatsAppOptions>(
            configuration.GetSection(WhatsAppOptions.SectionName));

        services.AddDatabaseFailover(configuration);

        services.AddDbContext<DeliveryDbContext>((provider, options) =>
            options
                .UseNpgsql(
                    provider.GetRequiredService<DatabaseFailoverState>().ActiveConnection,
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "delivery"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddHttpClient(MetaWhatsAppClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddScoped<IWhatsAppClient, MetaWhatsAppClient>();

        return services;
    }
}
