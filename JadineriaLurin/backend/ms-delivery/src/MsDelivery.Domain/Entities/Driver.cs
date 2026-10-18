using MsDelivery.Domain.Enums;
using MsDelivery.Domain.Exceptions;

namespace MsDelivery.Domain.Entities;

public class Driver
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public VehicleType VehicleType { get; set; }
    public CapacityLevel MaxCapacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<DeliveryOrder> Orders { get; set; } = new List<DeliveryOrder>();

    public bool Supports(CapacityLevel requiredCapacity) => IsActive && MaxCapacity >= requiredCapacity;

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del repartidor es obligatorio.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
