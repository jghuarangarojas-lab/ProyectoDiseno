using Iot.Domain.Exceptions;

namespace Iot.Domain.Entities;

public class BotanicalThreshold
{
    public const int MinHumidityFloor = 0;
    public const int MaxHumidityCeiling = 100;
    public const double MinTemperatureFloor = -50d;
    public const double MaxTemperatureCeiling = 80d;
    public const int MinAutoIrrigationSeconds = 1;
    public const int MaxAutoIrrigationSeconds = 60;

    public Guid Id { get; private set; }
    public string DeviceCode { get; private set; } = string.Empty;
    public int MinHumidityPercentage { get; private set; }
    public int MaxHumidityPercentage { get; private set; }
    public double MinTemperatureCelsius { get; private set; }
    public double MaxTemperatureCelsius { get; private set; }
    public int AutoIrrigationSeconds { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private BotanicalThreshold()
    {
    }

    public static BotanicalThreshold Create(
        string deviceCode,
        int minHumidityPercentage,
        int maxHumidityPercentage,
        double minTemperatureCelsius,
        double maxTemperatureCelsius,
        int autoIrrigationSeconds,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new IotDomainException("El código del dispositivo (DeviceCode) es obligatorio.");
        }

        Validate(
            minHumidityPercentage,
            maxHumidityPercentage,
            minTemperatureCelsius,
            maxTemperatureCelsius,
            autoIrrigationSeconds);

        return new BotanicalThreshold
        {
            Id = Guid.NewGuid(),
            DeviceCode = deviceCode.Trim(),
            MinHumidityPercentage = minHumidityPercentage,
            MaxHumidityPercentage = maxHumidityPercentage,
            MinTemperatureCelsius = minTemperatureCelsius,
            MaxTemperatureCelsius = maxTemperatureCelsius,
            AutoIrrigationSeconds = autoIrrigationSeconds,
            UpdatedAtUtc = utcNow
        };
    }

    public void Update(
        int minHumidityPercentage,
        int maxHumidityPercentage,
        double minTemperatureCelsius,
        double maxTemperatureCelsius,
        int autoIrrigationSeconds,
        DateTime utcNow)
    {
        Validate(
            minHumidityPercentage,
            maxHumidityPercentage,
            minTemperatureCelsius,
            maxTemperatureCelsius,
            autoIrrigationSeconds);

        MinHumidityPercentage = minHumidityPercentage;
        MaxHumidityPercentage = maxHumidityPercentage;
        MinTemperatureCelsius = minTemperatureCelsius;
        MaxTemperatureCelsius = maxTemperatureCelsius;
        AutoIrrigationSeconds = autoIrrigationSeconds;
        UpdatedAtUtc = utcNow;
    }

    private static void Validate(
        int minHumidityPercentage,
        int maxHumidityPercentage,
        double minTemperatureCelsius,
        double maxTemperatureCelsius,
        int autoIrrigationSeconds)
    {
        if (minHumidityPercentage is < MinHumidityFloor or > MaxHumidityCeiling)
        {
            throw new IotDomainException(
                $"La humedad mínima ({minHumidityPercentage}%) debe estar entre {MinHumidityFloor} y {MaxHumidityCeiling}.");
        }

        if (maxHumidityPercentage is < MinHumidityFloor or > MaxHumidityCeiling)
        {
            throw new IotDomainException(
                $"La humedad máxima ({maxHumidityPercentage}%) debe estar entre {MinHumidityFloor} y {MaxHumidityCeiling}.");
        }

        if (minHumidityPercentage >= maxHumidityPercentage)
        {
            throw new IotDomainException("La humedad mínima debe ser estrictamente menor que la humedad máxima.");
        }

        if (minTemperatureCelsius < MinTemperatureFloor || minTemperatureCelsius > MaxTemperatureCeiling)
        {
            throw new IotDomainException(
                $"La temperatura mínima ({minTemperatureCelsius}°C) debe estar entre {MinTemperatureFloor} y {MaxTemperatureCeiling}.");
        }

        if (maxTemperatureCelsius < MinTemperatureFloor || maxTemperatureCelsius > MaxTemperatureCeiling)
        {
            throw new IotDomainException(
                $"La temperatura máxima ({maxTemperatureCelsius}°C) debe estar entre {MinTemperatureFloor} y {MaxTemperatureCeiling}.");
        }

        if (minTemperatureCelsius >= maxTemperatureCelsius)
        {
            throw new IotDomainException("La temperatura mínima debe ser estrictamente menor que la temperatura máxima.");
        }

        if (autoIrrigationSeconds is < MinAutoIrrigationSeconds or > MaxAutoIrrigationSeconds)
        {
            throw new IotDomainException(
                $"La duración de riego automático ({autoIrrigationSeconds}s) debe estar entre " +
                $"{MinAutoIrrigationSeconds} y {MaxAutoIrrigationSeconds} segundos.");
        }
    }
}
