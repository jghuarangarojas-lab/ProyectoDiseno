namespace MsInventory.Domain.Entities;

public class InventoryEntry
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductPlant? Product { get; set; }
    public decimal Quantity { get; set; }
    public string Supplier { get; set; } = null!;
    public DateTime EntryDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
