using System.Text.RegularExpressions;
using Iot.Domain.Enums;
using Iot.Domain.Exceptions;

namespace Iot.Domain.Entities;

public class IotDevice
{
    public const int LowBatteryThresholdPercentage = 15;
    public const int MaxDeviceCodeLength = 64;
    public const int MaxMacAddressLength = 17;
    public const int MaxSectorAreaLength = 32;

    private static readonly Regex MacAddressPattern = new(
        "^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$",
        RegexOptions.Compiled);

    public string DeviceCode { get; private set; } = string.Empty;
    public string MacAddress { get; private set; } = string.Empty;
    public string CurrentSectorArea { get; private set; } = string.Empty;
    public DeviceStatus Status { get; private set; }
    public int BatteryPercentage { get; private set; }
    public DateTime? LastSeenAtUtc { get; private set; }
    public DateTime? LastIrrigationRequestedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public BotanicalThreshold? Threshold { get; private set; }

    private IotDevice()
    {
    }

    public static IotDevice Create(
        string deviceCode,
        string macAddress,
        string currentSectorArea,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new IotDomainException("El código del dispositivo (DeviceCode) es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(macAddress) || !MacAddressPattern.IsMatch(macAddress.Trim()))
        {
            throw new IotDomainException(
                $"La dirección MAC '{macAddress}' no tiene un formato válido (XX:XX:XX:XX:XX:XX).");
        }

        if (string.IsNullOrWhiteSpace(currentSectorArea))
        {
            throw new IotDomainException("El sector actual (CurrentSectorArea) es obligatorio.");
        }

        return new IotDevice
        {
            DeviceCode = deviceCode.Trim(),
            MacAddress = macAddress.Trim().ToUpperInvariant(),
            CurrentSectorArea = currentSectorArea.Trim(),
            Status = DeviceStatus.OFFLINE,
            BatteryPercentage = 100,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public bool IsBatteryLow => BatteryPercentage < LowBatteryThresholdPercentage;

    public bool IsStaleAsOf(DateTime utcNow, TimeSpan maximumSilence)
    {
        if (Status == DeviceStatus.OFFLINE || LastSeenAtUtc is null)
        {
            return false;
        }

        return utcNow - LastSeenAtUtc.Value > maximumSilence;
    }

    public void Relocate(string currentSectorArea, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(currentSectorArea))
        {
            throw new IotDomainException("El nuevo sector (CurrentSectorArea) es obligatorio.");
        }

        CurrentSectorArea = currentSectorArea.Trim();
        UpdatedAtUtc = utcNow;
    }

    public void ReportTelemetry(int batteryPercentage, DateTime utcNow)
    {
        if (batteryPercentage is < 0 or > 100)
        {
            throw new IotDomainException(
                $"El porcentaje de batería ({batteryPercentage}%) debe estar entre 0 y 100.");
        }

        BatteryPercentage = batteryPercentage;
        LastSeenAtUtc = utcNow;
        Status = DeviceStatus.ONLINE;
        UpdatedAtUtc = utcNow;
    }

    public void MarkAsOffline(DateTime utcNow)
    {
        Status = DeviceStatus.OFFLINE;
        UpdatedAtUtc = utcNow;
    }

    public void MarkIrrigationRequested(DateTime utcNow)
    {
        LastIrrigationRequestedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void AssignThreshold(BotanicalThreshold threshold, DateTime utcNow)
    {
        Threshold = threshold ?? throw new IotDomainException("El umbral botánico es obligatorio.");
        UpdatedAtUtc = utcNow;
    }
}
