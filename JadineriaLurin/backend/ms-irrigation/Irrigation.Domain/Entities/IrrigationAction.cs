using Irrigation.Domain.Enums;
using Irrigation.Domain.Exceptions;

namespace Irrigation.Domain.Entities;

public class IrrigationAction
{
    public const int PumpDurationLimitSeconds = 60;
    public const int MinDurationSeconds = 1;

    public Guid Id { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string SectorArea { get; private set; } = string.Empty;
    public TriggerType TriggerType { get; private set; }
    public int RequestedDurationSeconds { get; private set; }
    public int MaxPumpDurationSeconds { get; private set; }
    public int? ActualDurationSeconds { get; private set; }
    public IrrigationStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? FinishedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private IrrigationAction()
    {
    }

    public static IrrigationAction Create(
        string deviceId,
        string sectorArea,
        TriggerType triggerType,
        int requestedDurationSeconds,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new IrrigationDomainException("El identificador del dispositivo (DeviceId) es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(sectorArea))
        {
            throw new IrrigationDomainException("El sector a irrigar (SectorArea) es obligatorio.");
        }

        if (!Enum.IsDefined(triggerType))
        {
            throw new IrrigationDomainException("El tipo de disparo (TriggerType) no es válido.");
        }

        if (requestedDurationSeconds < MinDurationSeconds)
        {
            throw new IrrigationDomainException($"La duración solicitada debe ser al menos {MinDurationSeconds} segundo.");
        }

        if (requestedDurationSeconds > PumpDurationLimitSeconds)
        {
            throw new IrrigationDomainException(
                $"La duración solicitada ({requestedDurationSeconds}s) excede el límite de la bomba de {PumpDurationLimitSeconds} segundos.");
        }

        return new IrrigationAction
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId.Trim(),
            SectorArea = sectorArea.Trim(),
            TriggerType = triggerType,
            RequestedDurationSeconds = requestedDurationSeconds,
            MaxPumpDurationSeconds = PumpDurationLimitSeconds,
            Status = IrrigationStatus.PENDING,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public void MarkAsInProgress(DateTime utcNow)
    {
        EnsureTransition(IrrigationStatus.PENDING, IrrigationStatus.IN_PROGRESS, nameof(MarkAsInProgress));

        Status = IrrigationStatus.IN_PROGRESS;
        StartedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Complete(int actualDurationSeconds, DateTime utcNow)
    {
        EnsureTransition(IrrigationStatus.IN_PROGRESS, IrrigationStatus.COMPLETED, nameof(Complete));

        if (actualDurationSeconds < 0)
        {
            throw new IrrigationDomainException("La duración real medida no puede ser negativa.");
        }

        if (actualDurationSeconds > MaxPumpDurationSeconds)
        {
            throw new IrrigationDomainException(
                $"La duración real medida ({actualDurationSeconds}s) excede el límite de la bomba de {MaxPumpDurationSeconds} segundos.");
        }

        ActualDurationSeconds = actualDurationSeconds;
        Status = IrrigationStatus.COMPLETED;
        FinishedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Fail(string reason, DateTime utcNow)
    {
        if (Status is IrrigationStatus.COMPLETED or IrrigationStatus.CANCELLED or IrrigationStatus.FAILED)
        {
            throw new IrrigationDomainException(
                $"No se puede marcar como fallida una acción en estado {Status}.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new IrrigationDomainException("El motivo de la falla es obligatorio.");
        }

        FailureReason = reason.Trim();
        Status = IrrigationStatus.FAILED;
        FinishedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Cancel(string reason, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new IrrigationDomainException("El motivo de la cancelación es obligatorio.");
        }

        if (Status is not (IrrigationStatus.PENDING or IrrigationStatus.IN_PROGRESS))
        {
            throw new IrrigationDomainException(
                $"No se puede cancelar una acción en estado {Status}. Solo se cancelan acciones PENDING o IN_PROGRESS.");
        }

        CancellationReason = reason.Trim();
        Status = IrrigationStatus.CANCELLED;
        FinishedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void UpdateCancellationReason(string reason, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new IrrigationDomainException("El motivo de la cancelación es obligatorio.");
        }

        if (Status is not IrrigationStatus.CANCELLED)
        {
            throw new IrrigationDomainException(
                $"Solo se puede actualizar el motivo de cancelación de una acción CANCELLED. La acción {Id} está en estado {Status}.");
        }

        CancellationReason = reason.Trim();
        UpdatedAtUtc = utcNow;
    }

    public bool CanStartRemotely => Status is IrrigationStatus.PENDING;

    public bool RequiresDeviceCancellation => Status is IrrigationStatus.IN_PROGRESS;

    private void EnsureTransition(IrrigationStatus expected, IrrigationStatus target, string operation)
    {
        if (Status != expected)
        {
            throw new IrrigationDomainException(
                $"Transición inválida en {operation}: se esperaba {expected} pero la acción está en {Status}. La acción {Id} pasará a {target}.");
        }
    }
}
