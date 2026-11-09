using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;

namespace Iot.Application.Interfaces.Services;

public interface ITelemetryService
{
    Task<IotDeviceDto> ProcessTelemetryAsync(TelemetryInputDto input, CancellationToken cancellationToken = default);

    Task<IotDeviceDto?> HandleDeviceUnavailableAsync(
        string deviceCode,
        string reason,
        CancellationToken cancellationToken = default);
}
