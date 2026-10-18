using Iot.Application.DTOs.Responses;

namespace Iot.Application.Interfaces.Realtime;

public interface IIotStateNotifier
{
    Task NotifyDeviceUpdatedAsync(IotDeviceDto device, CancellationToken cancellationToken = default);

    Task NotifyTelemetryReceivedAsync(TelemetryRecordDto telemetry, CancellationToken cancellationToken = default);

    Task NotifyAlertRaisedAsync(DeviceAlertDto alert, CancellationToken cancellationToken = default);

    Task NotifyAlertResolvedAsync(DeviceAlertDto alert, CancellationToken cancellationToken = default);
}
