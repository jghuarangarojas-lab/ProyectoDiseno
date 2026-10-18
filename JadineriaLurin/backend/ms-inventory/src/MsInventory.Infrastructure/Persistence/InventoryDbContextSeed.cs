using Microsoft.EntityFrameworkCore;
using MsInventory.Domain.Entities;

namespace MsInventory.Infrastructure.Persistence;

public static class InventoryDbContextSeed
{
    public static async Task SeedAsync(InventoryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.ProductPlants.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;

        dbContext.ProductPlants.AddRange(
            new ProductPlant
            {
                Id = Guid.NewGuid(),
                Sku = "V001",
                CommonName = "Violeta Americana",
                ImageUrl = "/images/v001.jpg",
                Category = "Plantas",
                StockAvailable = 40,
                StockMinimum = 10,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductPlant
            {
                Id = Guid.NewGuid(),
                Sku = "R001",
                CommonName = "Rosa Roja Maceta 15cm",
                ImageUrl = "/images/r001.jpg",
                Category = "Plantas",
                StockAvailable = 8,
                StockMinimum = 12,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductPlant
            {
                Id = Guid.NewGuid(),
                Sku = "F001",
                CommonName = "Fertilizante Orgánico 1kg",
                ImageUrl = "/images/f001.jpg",
                Category = "Insumos",
                StockAvailable = 25,
                StockMinimum = 15,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductPlant
            {
                Id = Guid.NewGuid(),
                Sku = "M001",
                CommonName = "Maceta Cerámica 20cm",
                ImageUrl = "/images/m001.jpg",
                Category = "Macetas",
                StockAvailable = 5,
                StockMinimum = 8,
                CreatedAt = now,
                UpdatedAt = now
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
