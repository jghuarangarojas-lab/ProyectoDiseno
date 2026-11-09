using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Services;
using LurinSmart.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<FrontendSettings>(configuration.GetSection(FrontendSettings.SectionName));

        services.AddDatabaseFailover(configuration);

        services.AddDbContext<AuthDbContext>((provider, options) =>
            options.UseNpgsql(
                provider.GetRequiredService<DatabaseFailoverState>().ActiveConnection,
                npgsql => npgsql
                    .MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__ef_migrations_history", "auth"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}
