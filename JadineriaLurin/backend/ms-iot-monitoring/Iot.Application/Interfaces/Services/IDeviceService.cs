using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;

namespace Iot.Application.Interfaces.Services;

public interface IDeviceService
{
    const int DefaultTelemetryLimit = 20;
    const int MaxTelemetryLimit = 200;

    Task<IReadOnlyList<IotDeviceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IotDeviceDto> GetByCodeAsync(string deviceCode, CancellationToken cancellationToken = default);

    Task<IotDeviceDto> RegisterAsync(CreateDeviceDto dto, CancellationToken cancellationToken = default);

    Task<IotDeviceDto> RelocateAsync(string deviceCode, RelocateSensorDto dto, CancellationToken cancellationToken = default);

    Task<BotanicalThresholdDto> GetThresholdAsync(string deviceCode, CancellationToken cancellationToken = default);

    Task<BotanicalThresholdDto> UpdateThresholdAsync(
        string deviceCode,
        UpdateThresholdDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelemetryRecordDto>> GetTelemetryAsync(
        string deviceCode,
        int limit,
        CancellationToken cancellationToken = default);
}
