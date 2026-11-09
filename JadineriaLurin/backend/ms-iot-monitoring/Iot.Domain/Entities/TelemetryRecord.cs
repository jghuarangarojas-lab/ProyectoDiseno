using Iot.Domain.Exceptions;

namespace Iot.Domain.Entities;

public class TelemetryRecord
{
    public const int MinHumidityPercentage = 0;
    public const int MaxHumidityPercentage = 100;
    public const double MinTemperatureCelsius = -50d;
    public const double MaxTemperatureCelsius = 80d;

    public Guid Id { get; private set; }
    public string DeviceCode { get; private set; } = string.Empty;
    public DateTime RecordedAtUtc { get; private set; }
    public double HumidityPercentage { get; private set; }
    public double TemperatureCelsius { get; private set; }

    private TelemetryRecord()
    {
    }

    public static TelemetryRecord Create(
        string deviceCode,
        double humidityPercentage,
        double temperatureCelsius,
        DateTime recordedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new IotDomainException("El código del dispositivo (DeviceCode) es obligatorio.");
        }

        if (humidityPercentage < MinHumidityPercentage || humidityPercentage > MaxHumidityPercentage)
        {
            throw new IotDomainException(
                $"La humedad reportada ({humidityPercentage}%) debe estar entre {MinHumidityPercentage} y {MaxHumidityPercentage}.");
        }

        if (temperatureCelsius < MinTemperatureCelsius || temperatureCelsius > MaxTemperatureCelsius)
        {
            throw new IotDomainException(
                $"La temperatura reportada ({temperatureCelsius}°C) debe estar entre {MinTemperatureCelsius} y {MaxTemperatureCelsius}.");
        }

        return new TelemetryRecord
        {
            Id = Guid.NewGuid(),
            DeviceCode = deviceCode.Trim(),
            HumidityPercentage = humidityPercentage,
            TemperatureCelsius = temperatureCelsius,
            RecordedAtUtc = recordedAtUtc
        };
    }
}
