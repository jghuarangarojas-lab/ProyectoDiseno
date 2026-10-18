using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Responses;

public sealed record IotDeviceDto
{
    public string DeviceCode { get; init; } = string.Empty;
    public string MacAddress { get; init; } = string.Empty;
    public string CurrentSectorArea { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int BatteryPercentage { get; init; }
    public bool IsBatteryLow { get; init; }
    public DateTime? LastSeenAtUtc { get; init; }
    public DateTime? LastIrrigationRequestedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public BotanicalThresholdDto? Threshold { get; init; }

    public static IotDeviceDto From(IotDevice device)
    {
        return new IotDeviceDto
        {
            DeviceCode = device.DeviceCode,
            MacAddress = device.MacAddress,
            CurrentSectorArea = device.CurrentSectorArea,
            Status = device.Status.ToString(),
            BatteryPercentage = device.BatteryPercentage,
            IsBatteryLow = device.IsBatteryLow,
            LastSeenAtUtc = device.LastSeenAtUtc,
            LastIrrigationRequestedAtUtc = device.LastIrrigationRequestedAtUtc,
            UpdatedAtUtc = device.UpdatedAtUtc,
            Threshold = device.Threshold is null ? null : BotanicalThresholdDto.From(device.Threshold)
        };
    }
}
