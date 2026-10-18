namespace Irrigation.Application.Messaging;

public sealed record IrrigationCommandMessage(
    Guid ActionId,
    string DeviceId,
    string SectorArea,
    int RequestedDurationSeconds,
    int MaxPumpDurationSeconds,
    string TriggerType,
    DateTime IssuedAtUtc);

public sealed record IrrigationCancelCommandMessage(
    Guid ActionId,
    string DeviceId,
    string Reason,
    DateTime IssuedAtUtc);

public sealed record IrrigationAckMessage(
    Guid ActionId,
    string DeviceId,
    string Status,
    int? ActualDurationSeconds,
    string? Reason,
    DateTime TimestampUtc);
