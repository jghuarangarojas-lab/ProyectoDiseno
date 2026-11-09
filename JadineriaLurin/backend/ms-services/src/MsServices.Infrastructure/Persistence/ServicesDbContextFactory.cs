using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MsServices.Infrastructure.Persistence;

public sealed class ServicesDbContextFactory : IDesignTimeDbContextFactory<ServicesDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=lurin_smart;Username=postgres;Password=postgre";

    public ServicesDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("LURIN_DESIGN_CONNECTION")
            ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<ServicesDbContext>()
            .UseNpgsql(
                connection,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "services"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ServicesDbContext(options);
    }
}
