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

        services.AddDbContext<DeliveryDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("DeliveryDb"))
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
