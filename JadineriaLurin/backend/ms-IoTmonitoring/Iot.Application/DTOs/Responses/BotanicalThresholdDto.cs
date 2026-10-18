using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Responses;

public sealed record BotanicalThresholdDto
{
    public string DeviceCode { get; init; } = string.Empty;
    public int MinHumidityPercentage { get; init; }
    public int MaxHumidityPercentage { get; init; }
    public double MinTemperatureCelsius { get; init; }
    public double MaxTemperatureCelsius { get; init; }
    public int AutoIrrigationSeconds { get; init; }
    public DateTime UpdatedAtUtc { get; init; }

    public static BotanicalThresholdDto From(BotanicalThreshold threshold)
    {
        return new BotanicalThresholdDto
        {
            DeviceCode = threshold.DeviceCode,
            MinHumidityPercentage = threshold.MinHumidityPercentage,
            MaxHumidityPercentage = threshold.MaxHumidityPercentage,
            MinTemperatureCelsius = threshold.MinTemperatureCelsius,
            MaxTemperatureCelsius = threshold.MaxTemperatureCelsius,
            AutoIrrigationSeconds = threshold.AutoIrrigationSeconds,
            UpdatedAtUtc = threshold.UpdatedAtUtc
        };
    }
}
