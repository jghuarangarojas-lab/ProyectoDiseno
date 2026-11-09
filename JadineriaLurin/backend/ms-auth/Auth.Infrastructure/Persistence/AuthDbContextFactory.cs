using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Infrastructure.Persistence;

public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=lurin_smart;Username=postgres;Password=postgre";

    public AuthDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("LURIN_DESIGN_CONNECTION")
            ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(
                connection,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "auth"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AuthDbContext(options);
    }
}
