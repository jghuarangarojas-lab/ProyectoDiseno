using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MsDelivery.Infrastructure.Persistence;

public sealed class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=lurin_smart;Username=postgres;Password=postgre";

    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("LURIN_DESIGN_CONNECTION")
            ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseNpgsql(
                connection,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "delivery"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DeliveryDbContext(options);
    }
}
