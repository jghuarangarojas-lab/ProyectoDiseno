using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;
using Iot.Application.Exceptions;
using Iot.Application.Interfaces.Realtime;
using Iot.Application.Interfaces.Repositories;
using Iot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Iot.Application.Services;

public sealed class AlertService : IAlertService
{
    private readonly IDeviceAlertRepository _alertRepository;
    private readonly IIotStateNotifier _notifier;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IDeviceAlertRepository alertRepository,
        IIotStateNotifier notifier,
        TimeProvider timeProvider,
        ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository;
        _notifier = notifier;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DeviceAlertDto>> GetAlertsAsync(
        string? deviceCode,
        bool includeResolved,
        CancellationToken cancellationToken = default)
    {
        var normalizedDeviceCode = string.IsNullOrWhiteSpace(deviceCode) ? null : deviceCode.Trim();
        var alerts = await _alertRepository.GetAllAsync(normalizedDeviceCode, includeResolved, cancellationToken);
        return alerts.Select(DeviceAlertDto.From).ToList();
    }

    public async Task<DeviceAlertDto> ResolveAlertAsync(
        Guid id,
        ResolveAlertDto dto,
        CancellationToken cancellationToken = default)
    {
        var alert = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new IotNotFoundException($"No existe ninguna alerta con id '{id}'.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        alert.Resolve(dto.ResolutionNote, utcNow);
        await _alertRepository.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyAlertResolvedAsync(DeviceAlertDto.From(alert), cancellationToken);

        _logger.LogInformation(
            "Alerta {AlertId} ({IssueType}) del dispositivo '{DeviceCode}' marcada como resuelta",
            alert.Id,
            alert.IssueType,
            alert.DeviceCode);

        return DeviceAlertDto.From(alert);
    }
}
