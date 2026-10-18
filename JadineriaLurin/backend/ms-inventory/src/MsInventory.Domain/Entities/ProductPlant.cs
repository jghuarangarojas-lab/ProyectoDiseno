namespace MsInventory.Domain.Entities;

public class ProductPlant
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = null!;
    public string CommonName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = null!;
    public decimal StockAvailable { get; set; }
    public decimal StockMinimum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InventoryEntry> Entries { get; set; } = new List<InventoryEntry>();
    public ICollection<MermaRecord> MermaRecords { get; set; } = new List<MermaRecord>();

    public bool IsBelowMinimum => StockAvailable < StockMinimum;

    public void AddStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new Exceptions.DomainException("La cantidad a ingresar debe ser mayor a cero.");

        StockAvailable += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new Exceptions.DomainException("La cantidad a descontar debe ser mayor a cero.");

        if (quantity > StockAvailable)
            throw new Exceptions.InsufficientStockException(
                Sku, CommonName, quantity, StockAvailable);

        StockAvailable -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
