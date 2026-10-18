using System.ComponentModel.DataAnnotations;
using MsInventory.Domain.Enums;

namespace MsInventory.Application.DTOs;

public sealed class RegisterMermaCommand
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal QuantityLost { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = null!;

    public Stream? EvidencePhotoStream { get; set; }
    public string? EvidencePhotoFileName { get; set; }
}

public sealed record MermaRecordDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string CommonName,
    decimal QuantityLost,
    string Reason,
    string? EvidencePhotoUrl,
    string ManagerReviewStatus,
    DateTime CreatedAt);

public sealed class ReviewMermaRequest
{
    [Required]
    public string Status { get; set; } = null!;

    public ManagerReviewStatus ParseStatus() => ManagerReviewStatusExtensions.FromCode(Status);
}
