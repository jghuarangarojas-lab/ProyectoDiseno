using Irrigation.Domain.Entities;

namespace Irrigation.Application.DTOs.Responses;

public sealed record IrrigationActionDto(
    Guid Id,
    string DeviceId,
    string SectorArea,
    string TriggerType,
    int RequestedDurationSeconds,
    int MaxPumpDurationSeconds,
    int? ActualDurationSeconds,
    string Status,
    string? CancellationReason,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? FinishedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static IrrigationActionDto From(IrrigationAction action)
    {
        return new IrrigationActionDto(
            action.Id,
            action.DeviceId,
            action.SectorArea,
            action.TriggerType.ToString(),
            action.RequestedDurationSeconds,
            action.MaxPumpDurationSeconds,
            action.ActualDurationSeconds,
            action.Status.ToString(),
            action.CancellationReason,
            action.FailureReason,
            action.CreatedAtUtc,
            action.StartedAtUtc,
            action.FinishedAtUtc,
            action.UpdatedAtUtc);
    }
}
