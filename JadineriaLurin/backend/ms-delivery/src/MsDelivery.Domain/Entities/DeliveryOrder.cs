using MsDelivery.Domain.Enums;
using MsDelivery.Domain.Exceptions;

namespace MsDelivery.Domain.Entities;

public class DeliveryOrder
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public string Address { get; set; } = null!;
    public DateOnly DeliveryDate { get; set; }
    public TimeOnly DeliveryTime { get; set; }
    public string Phone { get; set; } = null!;
    public string Reference { get; set; } = null!;
    public CapacityLevel RequiredCapacity { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }
    public string? FailureReason { get; set; }
    public string? ProofPhotoUrl { get; set; }
    public string? DigitalSignatureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool HasDeliveryProof =>
        !string.IsNullOrWhiteSpace(ProofPhotoUrl) || !string.IsNullOrWhiteSpace(DigitalSignatureUrl);

    public static bool IsTerminal(DeliveryStatus status) =>
        status is DeliveryStatus.Delivered or DeliveryStatus.Failed;

    public bool CanTransitionTo(DeliveryStatus target) => (Status, target) switch
    {
        (DeliveryStatus.Pending, DeliveryStatus.Dispatched) => true,
        (DeliveryStatus.Pending, DeliveryStatus.Delivered) => true,
        (DeliveryStatus.Pending, DeliveryStatus.Failed) => true,
        (DeliveryStatus.Dispatched, DeliveryStatus.Delivered) => true,
        (DeliveryStatus.Dispatched, DeliveryStatus.Failed) => true,
        _ => false
    };

    public void AssignDriver(Driver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);

        if (Status != DeliveryStatus.Pending)
            throw new InvalidStatusTransitionException(Status, DeliveryStatus.Pending);

        if (!driver.Supports(RequiredCapacity))
            throw new DomainException(driver.IsActive
                ? $"El repartidor '{driver.Name}' no soporta la capacidad requerida ({RequiredCapacity.ToCode()}); su capacidad máxima es {driver.MaxCapacity.ToCode()}."
                : $"El repartidor '{driver.Name}' no está activo.");

        DriverId = driver.Id;
        Driver = driver;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnassignDriver()
    {
        if (Status != DeliveryStatus.Pending)
            throw new InvalidStatusTransitionException(Status, DeliveryStatus.Pending);

        DriverId = null;
        Driver = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDriver(Driver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);

        if (IsTerminal(Status))
            throw new DomainException($"No se puede reasignar una orden en estado terminal ({Status.ToCode()}).");

        if (!driver.Supports(RequiredCapacity))
            throw new DomainException(driver.IsActive
                ? $"El repartidor '{driver.Name}' no soporta la capacidad requerida ({RequiredCapacity.ToCode()}); su capacidad máxima es {driver.MaxCapacity.ToCode()}."
                : $"El repartidor '{driver.Name}' no está activo.");

        DriverId = driver.Id;
        Driver = driver;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDispatched()
    {
        EnsureTransition(DeliveryStatus.Dispatched);

        if (DriverId is null)
            throw new DomainException("No se puede despachar una orden sin repartidor asignado.");

        Status = DeliveryStatus.Dispatched;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDelivered(string? proofPhotoUrl = null, string? digitalSignatureUrl = null)
    {
        EnsureTransition(DeliveryStatus.Delivered);

        if (!string.IsNullOrWhiteSpace(proofPhotoUrl))
            ProofPhotoUrl = proofPhotoUrl;

        if (!string.IsNullOrWhiteSpace(digitalSignatureUrl))
            DigitalSignatureUrl = digitalSignatureUrl;

        if (!HasDeliveryProof)
            throw new ProofOfDeliveryMissingException();

        Status = DeliveryStatus.Delivered;
        FailureReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string failureReason)
    {
        EnsureTransition(DeliveryStatus.Failed);

        if (string.IsNullOrWhiteSpace(failureReason))
            throw new DomainException("El motivo de falla (FailureReason) es obligatorio para marcar la orden como FAILED.");

        FailureReason = failureReason.Trim();
        Status = DeliveryStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureTransition(DeliveryStatus target)
    {
        if (!CanTransitionTo(target))
            throw new InvalidStatusTransitionException(Status, target);
    }
}
