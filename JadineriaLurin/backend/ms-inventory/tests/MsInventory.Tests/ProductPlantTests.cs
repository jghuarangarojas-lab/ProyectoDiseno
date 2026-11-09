using Xunit;
using MsInventory.Domain.Entities;

namespace MsInventory.Tests;

public class ProductPlantTests
{
    [Fact]
    public void LowStock_WhenStockBelowMinimum_ReturnsTrue()
    {
        var product = new ProductPlant
        {
            Id = System.Guid.NewGuid(),
            Sku = ""P1"",
            CommonName = ""Test"",
            StockAvailable = 5,
            StockMinimum = 10
        };
        Assert.True(product.StockAvailable < product.StockMinimum);
    }
}
