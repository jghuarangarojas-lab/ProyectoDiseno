using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LurinSmart.Persistence;

public static class DatabaseFailoverServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseFailover(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseFailoverOptions>(
            configuration.GetSection(DatabaseFailoverOptions.SectionName));

        services.TryAddSingleton<DatabaseFailoverState>();
        services.AddHostedService<DatabaseFailoverMonitor>();

        return services;
    }
}
