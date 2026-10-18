namespace MsInventory.Infrastructure.Notifications;

public sealed record LowStockAlertPayload(
    Guid ProductId,
    string Sku,
    string CommonName,
    string Category,
    decimal StockAvailable,
    decimal StockMinimum,
    decimal SuggestedOrderQuantity,
    DateTime DetectedAtUtc);
