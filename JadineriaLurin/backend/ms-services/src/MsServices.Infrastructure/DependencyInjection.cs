using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsServices.Application.Abstractions;
using MsServices.Application.Interfaces.External;
using MsServices.Domain.Abstractions;
using MsServices.Infrastructure.Billing;
using MsServices.Infrastructure.Pdf;
using MsServices.Infrastructure.Persistence;
using MsServices.Infrastructure.Persistence.Repositories;
using MsServices.Infrastructure.Storage;

namespace MsServices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalFileStorageOptions>(
            configuration.GetSection(LocalFileStorageOptions.SectionName));
        services.Configure<ContractPdfOptions>(
            configuration.GetSection(ContractPdfOptions.SectionName));
        services.Configure<MsCommercialOptions>(
            configuration.GetSection(MsCommercialOptions.SectionName));

        services.AddDbContext<ServicesDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("ServicesDb"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IServiceProjectRepository, ServiceProjectRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IContractPdfService, ContractPdfService>();
        services.AddScoped<IBillingIntegration, BillingIntegration>();

        var commercialOptions = configuration.GetSection(MsCommercialOptions.SectionName)
                .Get<MsCommercialOptions>()
            ?? new MsCommercialOptions();

        services.AddHttpClient(BillingIntegration.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(commercialOptions.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, commercialOptions.TimeoutSeconds));
        });

        return services;
    }
}
