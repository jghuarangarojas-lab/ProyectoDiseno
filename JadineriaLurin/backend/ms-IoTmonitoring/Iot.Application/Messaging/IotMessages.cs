namespace Iot.Application.Messaging;

public sealed record TelemetryMessage
{
    public string DeviceCode { get; init; } = string.Empty;
    public double HumidityPercentage { get; init; }
    public double TemperatureCelsius { get; init; }
    public int BatteryPercentage { get; init; }
    public DateTime? SentAtUtc { get; init; }
}

public sealed record LastWillMessage
{
    public string DeviceCode { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
