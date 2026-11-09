using System.ComponentModel.DataAnnotations;
using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Requests;

public sealed record UpdateThresholdDto
{
    [Range(BotanicalThreshold.MinHumidityFloor, BotanicalThreshold.MaxHumidityCeiling, ErrorMessage = "MinHumidityPercentage debe estar entre 0 y 100.")]
    public int? MinHumidityPercentage { get; init; }

    [Range(BotanicalThreshold.MinHumidityFloor, BotanicalThreshold.MaxHumidityCeiling, ErrorMessage = "MaxHumidityPercentage debe estar entre 0 y 100.")]
    public int? MaxHumidityPercentage { get; init; }

    [Range(BotanicalThreshold.MinTemperatureFloor, BotanicalThreshold.MaxTemperatureCeiling, ErrorMessage = "MinTemperatureCelsius debe estar entre -50 y 80.")]
    public double? MinTemperatureCelsius { get; init; }

    [Range(BotanicalThreshold.MinTemperatureFloor, BotanicalThreshold.MaxTemperatureCeiling, ErrorMessage = "MaxTemperatureCelsius debe estar entre -50 y 80.")]
    public double? MaxTemperatureCelsius { get; init; }

    [Range(BotanicalThreshold.MinAutoIrrigationSeconds, BotanicalThreshold.MaxAutoIrrigationSeconds, ErrorMessage = "AutoIrrigationSeconds debe estar entre 1 y 60.")]
    public int? AutoIrrigationSeconds { get; init; }
}
