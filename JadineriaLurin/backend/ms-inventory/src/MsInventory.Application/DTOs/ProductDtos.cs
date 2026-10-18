using System.ComponentModel.DataAnnotations;

namespace MsInventory.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string CommonName,
    string? ImageUrl,
    string Category,
    decimal StockAvailable,
    decimal StockMinimum,
    bool IsBelowMinimum,
    DateTime UpdatedAt);

public sealed class ProductSearchQuery
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed record DeductStockRequest(
    [Required] Guid ProductId,
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Quantity,
    string? Reason);

public sealed record DeductStockResultDto(
    Guid ProductId,
    string Sku,
    string CommonName,
    decimal StockAvailable,
    decimal StockMinimum,
    bool LowStockAlertTriggered);

public sealed record ShoppingListItemDto(
    Guid ProductId,
    string Sku,
    string CommonName,
    string Category,
    string? ImageUrl,
    decimal StockAvailable,
    decimal StockMinimum,
    decimal SuggestedOrderQuantity);
