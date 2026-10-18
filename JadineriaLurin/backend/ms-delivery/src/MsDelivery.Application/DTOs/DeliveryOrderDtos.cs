using System.ComponentModel.DataAnnotations;

namespace MsDelivery.Application.DTOs;

public sealed class CreateDeliveryOrderRequest
{
    [Required]
    public Guid SaleId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = null!;

    [Required]
    public DateOnly DeliveryDate { get; set; }

    [Required]
    public TimeOnly DeliveryTime { get; set; }

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Reference { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string RequiredCapacity { get; set; } = null!;
}

public sealed class DeliveryOrderQuery
{
    public string? Status { get; set; }
    public Guid? DriverId { get; set; }
    public DateOnly? DeliveryDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class DeliveryOrderPageDto(
    IReadOnlyList<DeliveryOrderDto> items,
    int totalCount,
    int page,
    int pageSize)
{
    public IReadOnlyList<DeliveryOrderDto> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
}

public sealed record DeliveryOrderDto(
    Guid Id,
    Guid SaleId,
    string Address,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    string Phone,
    string Reference,
    string RequiredCapacity,
    string Status,
    Guid? DriverId,
    string? DriverName,
    string? VehicleType,
    string? FailureReason,
    string? ProofPhotoUrl,
    string? DigitalSignatureUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class ReassignOrderRequest
{
    [Required]
    public Guid DriverId { get; set; }
}

public sealed class UpdateDeliveryOrderStatusCommand
{
    [Required]
    public string Status { get; set; } = null!;

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    public Stream? ProofPhotoStream { get; set; }
    public string? ProofPhotoFileName { get; set; }

    public Stream? DigitalSignatureStream { get; set; }
    public string? DigitalSignatureFileName { get; set; }
}
