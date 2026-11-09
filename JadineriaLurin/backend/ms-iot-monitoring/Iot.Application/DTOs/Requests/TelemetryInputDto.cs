using System.ComponentModel.DataAnnotations;
using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Requests;

public sealed record TelemetryInputDto
{
    [Required(ErrorMessage = "DeviceCode es obligatorio.")]
    [StringLength(IotDevice.MaxDeviceCodeLength, MinimumLength = 1, ErrorMessage = "DeviceCode debe tener entre 1 y 64 caracteres.")]
    public string DeviceCode { get; init; } = string.Empty;

    [Range(TelemetryRecord.MinHumidityPercentage, TelemetryRecord.MaxHumidityPercentage, ErrorMessage = "HumidityPercentage debe estar entre 0 y 100.")]
    public double HumidityPercentage { get; init; }

    [Range(TelemetryRecord.MinTemperatureCelsius, TelemetryRecord.MaxTemperatureCelsius, ErrorMessage = "TemperatureCelsius debe estar entre -50 y 80.")]
    public double TemperatureCelsius { get; init; }

    [Range(0, 100, ErrorMessage = "BatteryPercentage debe estar entre 0 y 100.")]
    public int BatteryPercentage { get; init; }

    public DateTime? SentAtUtc { get; init; }
}
