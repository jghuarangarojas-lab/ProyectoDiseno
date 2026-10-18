using Microsoft.EntityFrameworkCore;
using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;

namespace MsDelivery.Infrastructure.Persistence;

public static class DeliveryDbContextSeed
{
    public static async Task SeedAsync(DeliveryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Drivers.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;

        dbContext.Drivers.AddRange(
            new Driver
            {
                Id = Guid.NewGuid(),
                Name = "Carlos Mendoza",
                Phone = "+51987654321",
                VehicleType = VehicleType.Motorcycle,
                MaxCapacity = CapacityLevel.Small,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Driver
            {
                Id = Guid.NewGuid(),
                Name = "Rosa Quispe",
                Phone = "+51987111222",
                VehicleType = VehicleType.Van,
                MaxCapacity = CapacityLevel.Medium,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Driver
            {
                Id = Guid.NewGuid(),
                Name = "Jorge Huamán",
                Phone = "+51987333444",
                VehicleType = VehicleType.Truck,
                MaxCapacity = CapacityLevel.Large,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Driver
            {
                Id = Guid.NewGuid(),
                Name = "María Torres",
                Phone = "+51987555666",
                VehicleType = VehicleType.Motorcycle,
                MaxCapacity = CapacityLevel.Small,
                IsActive = false,
                CreatedAt = now,
                UpdatedAt = now
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
