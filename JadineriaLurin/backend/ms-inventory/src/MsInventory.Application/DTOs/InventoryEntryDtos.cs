using System.ComponentModel.DataAnnotations;

namespace MsInventory.Application.DTOs;

public sealed class RegisterEntryBatchRequest
{
    [Required]
    [MaxLength(200)]
    public string Supplier { get; set; } = null!;

    public DateTime? EntryDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "El lote debe contener al menos un item.")]
    public List<EntryItemRequest> Items { get; set; } = new();
}

public sealed class EntryItemRequest
{
    public Guid? ProductId { get; set; }

    [MaxLength(50)]
    public string? Sku { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Quantity { get; set; }
}

public sealed record InventoryEntryDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string CommonName,
    decimal Quantity,
    string Supplier,
    DateTime EntryDate,
    DateTime? ExpirationDate);
