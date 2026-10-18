using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Responses;

public sealed record TelemetryRecordDto
{
    public Guid Id { get; init; }
    public string DeviceCode { get; init; } = string.Empty;
    public DateTime RecordedAtUtc { get; init; }
    public double HumidityPercentage { get; init; }
    public double TemperatureCelsius { get; init; }

    public static TelemetryRecordDto From(TelemetryRecord record)
    {
        return new TelemetryRecordDto
        {
            Id = record.Id,
            DeviceCode = record.DeviceCode,
            RecordedAtUtc = record.RecordedAtUtc,
            HumidityPercentage = record.HumidityPercentage,
            TemperatureCelsius = record.TemperatureCelsius
        };
    }
}
