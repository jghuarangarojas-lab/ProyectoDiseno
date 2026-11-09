using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;

namespace Iot.Application.Interfaces.Services;

public interface IAlertService
{
    Task<IReadOnlyList<DeviceAlertDto>> GetAlertsAsync(
        string? deviceCode,
        bool includeResolved,
        CancellationToken cancellationToken = default);

    Task<DeviceAlertDto> ResolveAlertAsync(Guid id, ResolveAlertDto dto, CancellationToken cancellationToken = default);
}
