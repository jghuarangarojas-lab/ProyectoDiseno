using Iot.Api.Hubs;
using Iot.Application.DTOs.Responses;
using Iot.Application.Interfaces.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Iot.Api.Realtime;

public sealed class IotHubNotifier : IIotStateNotifier
{
    public const string DeviceUpdatedMethod = "deviceUpdated";
    public const string TelemetryReceivedMethod = "telemetryReceived";
    public const string AlertRaisedMethod = "alertRaised";
    public const string AlertResolvedMethod = "alertResolved";

    private readonly IHubContext<IotHub> _hubContext;
    private readonly ILogger<IotHubNotifier> _logger;

    public IotHubNotifier(IHubContext<IotHub> hubContext, ILogger<IotHubNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task NotifyDeviceUpdatedAsync(IotDeviceDto device, CancellationToken cancellationToken = default)
    {
        return PublishAsync(DeviceUpdatedMethod, device, device.DeviceCode, cancellationToken);
    }

    public Task NotifyTelemetryReceivedAsync(TelemetryRecordDto telemetry, CancellationToken cancellationToken = default)
    {
        return PublishAsync(TelemetryReceivedMethod, telemetry, telemetry.DeviceCode, cancellationToken);
    }

    public Task NotifyAlertRaisedAsync(DeviceAlertDto alert, CancellationToken cancellationToken = default)
    {
        return PublishAsync(AlertRaisedMethod, alert, alert.DeviceCode, cancellationToken);
    }

    public Task NotifyAlertResolvedAsync(DeviceAlertDto alert, CancellationToken cancellationToken = default)
    {
        return PublishAsync(AlertResolvedMethod, alert, alert.DeviceCode, cancellationToken);
    }

    private async Task PublishAsync<T>(
        string methodName,
        T payload,
        string deviceCode,
        CancellationToken cancellationToken)
    {
        var groups = new[]
        {
            IotHub.DeviceGroup(deviceCode),
            IotHub.DashboardGroup
        };

        await _hubContext.Clients.Groups(groups).SendAsync(methodName, payload, cancellationToken);

        _logger.LogDebug(
            "Evento '{MethodName}' del dispositivo '{DeviceCode}' emitido a los grupos {Groups}",
            methodName,
            deviceCode,
            string.Join(", ", groups));
    }
}
