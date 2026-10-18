using System.ComponentModel.DataAnnotations;

namespace MsDelivery.Application.DTOs;

public sealed class CreateDriverRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string VehicleType { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string MaxCapacity { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDriverStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}

public sealed record DriverDto(
    Guid Id,
    string Name,
    string Phone,
    string VehicleType,
    string MaxCapacity,
    bool IsActive,
    int ActiveOrders,
    DateTime CreatedAt,
    DateTime UpdatedAt);
