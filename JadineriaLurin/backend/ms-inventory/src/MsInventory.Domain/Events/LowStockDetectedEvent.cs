namespace MsInventory.Domain.Events;

public sealed record LowStockDetectedEvent(
    Guid ProductId,
    string Sku,
    string CommonName,
    string Category,
    decimal StockAvailable,
    decimal StockMinimum)
{
    public decimal SuggestedOrderQuantity => StockMinimum - StockAvailable;
    public DateTime DetectedAtUtc { get; } = DateTime.UtcNow;
}
