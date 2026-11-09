using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MsInventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=lurin_smart;Username=postgres;Password=postgre";

    public InventoryDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("LURIN_DESIGN_CONNECTION")
            ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(
                connection,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "inventory"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new InventoryDbContext(options);
    }
}
