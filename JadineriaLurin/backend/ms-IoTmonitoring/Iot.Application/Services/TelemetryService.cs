using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;
using Iot.Application.Exceptions;
using Iot.Application.Interfaces.External;
using Iot.Application.Interfaces.Realtime;
using Iot.Application.Interfaces.Repositories;
using Iot.Application.Interfaces.Services;
using Iot.Domain.Entities;
using Iot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Iot.Application.Services;

public sealed class TelemetryService : ITelemetryService
{
    public const int RuleOfThreeReadings = 3;

    private static readonly TimeSpan IrrigationRequestCooldown = TimeSpan.FromMinutes(5);

    private readonly IIotDeviceRepository _deviceRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IDeviceAlertRepository _alertRepository;
    private readonly IIrrigationClient _irrigationClient;
    private readonly IIotStateNotifier _notifier;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(
        IIotDeviceRepository deviceRepository,
        ITelemetryRepository telemetryRepository,
        IDeviceAlertRepository alertRepository,
        IIrrigationClient irrigationClient,
        IIotStateNotifier notifier,
        TimeProvider timeProvider,
        ILogger<TelemetryService> logger)
    {
        _deviceRepository = deviceRepository;
        _telemetryRepository = telemetryRepository;
        _alertRepository = alertRepository;
        _irrigationClient = irrigationClient;
        _notifier = notifier;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IotDeviceDto> ProcessTelemetryAsync(
        TelemetryInputDto input,
        CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var device = await _deviceRepository.GetByCodeAsync(input.DeviceCode, cancellationToken)
            ?? throw new IotNotFoundException(
                $"No existe ningún dispositivo IoT registrado con código '{input.DeviceCode}'.");

        var record = TelemetryRecord.Create(
            device.DeviceCode,
            input.HumidityPercentage,
            input.TemperatureCelsius,
            input.SentAtUtc ?? utcNow);

        await _telemetryRepository.AddAsync(record, cancellationToken);

        var wasOffline = device.Status == DeviceStatus.OFFLINE;
        device.ReportTelemetry(input.BatteryPercentage, utcNow);
        await _deviceRepository.SaveChangesAsync(cancellationToken);

        if (wasOffline)
        {
            await _notifier.NotifyDeviceUpdatedAsync(IotDeviceDto.From(device), cancellationToken);
        }

        await _notifier.NotifyTelemetryReceivedAsync(TelemetryRecordDto.From(record), cancellationToken);

        if (device.IsBatteryLow)
        {
            await RaiseAlertIfMissingAsync(
                device,
                AlertIssueType.LOW_BATTERY,
                $"Batería al {device.BatteryPercentage}% (umbral de alerta {IotDevice.LowBatteryThresholdPercentage}%).",
                utcNow,
                cancellationToken);
        }

        await EvaluateRuleOfThreeAsync(device, utcNow, cancellationToken);

        return IotDeviceDto.From(device);
    }

    public async Task<IotDeviceDto?> HandleDeviceUnavailableAsync(
        string deviceCode,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new IotApplicationException("El código del dispositivo es obligatorio para registrar la desconexión.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var device = await _deviceRepository.GetByCodeAsync(deviceCode.Trim(), cancellationToken);

        if (device is null)
        {
            _logger.LogWarning(
                "Se reportó desconexión del dispositivo '{DeviceCode}' pero no está registrado",
                deviceCode);
            return null;
        }

        if (device.Status != DeviceStatus.OFFLINE)
        {
            device.MarkAsOffline(utcNow);
            await _deviceRepository.SaveChangesAsync(cancellationToken);
            await _notifier.NotifyDeviceUpdatedAsync(IotDeviceDto.From(device), cancellationToken);
        }

        var message = $"Hardware desconectado: {reason}. Última telemetría registrada: " +
                      $"{device.LastSeenAtUtc?.ToString("u") ?? "sin registros"}.";

        await RaiseAlertIfMissingAsync(
            device,
            AlertIssueType.HARDWARE_DISCONNECTED,
            message,
            utcNow,
            cancellationToken);

        _logger.LogWarning("Dispositivo '{DeviceCode}' marcado como OFFLINE: {Reason}", device.DeviceCode, reason);

        return IotDeviceDto.From(device);
    }

    private async Task EvaluateRuleOfThreeAsync(
        IotDevice device,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (device.Threshold is null)
        {
            return;
        }

        var lastReadings = await _telemetryRepository.GetLastAsync(
            device.DeviceCode,
            RuleOfThreeReadings,
            cancellationToken);

        if (lastReadings.Count < RuleOfThreeReadings)
        {
            return;
        }

        var minimumHumidity = device.Threshold.MinHumidityPercentage;
        var belowThreshold = lastReadings.Count(reading => reading.HumidityPercentage < minimumHumidity);

        if (belowThreshold < RuleOfThreeReadings)
        {
            return;
        }

        if (device.LastIrrigationRequestedAtUtc is not null &&
            utcNow - device.LastIrrigationRequestedAtUtc.Value < IrrigationRequestCooldown)
        {
            _logger.LogInformation(
                "Regla de las 3 lecturas cumplida para '{DeviceCode}' pero se respeta el periodo de gracia de {CooldownMinutes} min",
                device.DeviceCode,
                IrrigationRequestCooldown.TotalMinutes);
            return;
        }

        var request = new IrrigationRequest(
            device.DeviceCode,
            device.CurrentSectorArea,
            device.Threshold.AutoIrrigationSeconds);

        _logger.LogWarning(
            "Regla de las 3 lecturas disparada para '{DeviceCode}': {Count} lecturas por debajo de {Minimum}%" +
            " en el sector '{Sector}'. Solicitando riego automático a ms-irrigation",
            device.DeviceCode,
            belowThreshold,
            minimumHumidity,
            device.CurrentSectorArea);

        IrrigationRequestResult result;

        try
        {
            result = await _irrigationClient.RequestIrrigationAsync(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo contactar con ms-irrigation para el dispositivo '{DeviceCode}'",
                device.DeviceCode);

            await RaiseAlertIfMissingAsync(
                device,
                AlertIssueType.IRRIGATION_REQUEST_FAILED,
                $"No se pudo solicitar el riego automático a ms-irrigation: {exception.Message}",
                utcNow,
                cancellationToken);
            return;
        }

        if (!result.Succeeded)
        {
            _logger.LogError(
                "ms-irrigation rechazó la solicitud de riego para '{DeviceCode}': {Error}",
                device.DeviceCode,
                result.Error);

            await RaiseAlertIfMissingAsync(
                device,
                AlertIssueType.IRRIGATION_REQUEST_FAILED,
                $"ms-irrigation rechazó la solicitud de riego: {result.Error}",
                utcNow,
                cancellationToken);
            return;
        }

        device.MarkIrrigationRequested(utcNow);
        await _deviceRepository.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyDeviceUpdatedAsync(IotDeviceDto.From(device), cancellationToken);

        _logger.LogInformation(
            "Riego automático solicitado para '{DeviceCode}' en '{Sector}' durante {Seconds}s (acción {ActionId})",
            device.DeviceCode,
            device.CurrentSectorArea,
            device.Threshold.AutoIrrigationSeconds,
            result.ActionId);
    }

    private async Task<DeviceAlert?> RaiseAlertIfMissingAsync(
        IotDevice device,
        AlertIssueType issueType,
        string message,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var alreadyOpen = await _alertRepository.ExistsUnresolvedAsync(
            device.DeviceCode,
            issueType,
            cancellationToken);

        if (alreadyOpen)
        {
            return null;
        }

        var alert = DeviceAlert.Create(device.DeviceCode, issueType, message, utcNow);
        await _alertRepository.AddAsync(alert, cancellationToken);
        await _alertRepository.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyAlertRaisedAsync(DeviceAlertDto.From(alert), cancellationToken);

        _logger.LogWarning(
            "Alerta {IssueType} generada para '{DeviceCode}': {Message}",
            issueType,
            device.DeviceCode,
            message);

        return alert;
    }
}
