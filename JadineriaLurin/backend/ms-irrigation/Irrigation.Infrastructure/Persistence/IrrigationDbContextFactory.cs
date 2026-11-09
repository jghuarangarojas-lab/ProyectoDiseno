using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Irrigation.Infrastructure.Persistence;

public sealed class IrrigationDbContextFactory : IDesignTimeDbContextFactory<IrrigationDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=lurin_smart;Username=postgres;Password=postgre";

    public IrrigationDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("LURIN_DESIGN_CONNECTION")
            ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<IrrigationDbContext>()
            .UseNpgsql(
                connection,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "irrigation"))
            .Options;

        return new IrrigationDbContext(options);
    }
}
