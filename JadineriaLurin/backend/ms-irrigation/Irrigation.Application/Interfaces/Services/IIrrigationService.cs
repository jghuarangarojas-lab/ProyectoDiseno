using Irrigation.Application.DTOs.Requests;
using Irrigation.Application.DTOs.Responses;
using Irrigation.Application.Messaging;

namespace Irrigation.Application.Interfaces.Services;

public interface IIrrigationService
{
    Task<IrrigationActionDto> StartAsync(StartIrrigationDto dto, CancellationToken cancellationToken = default);

    Task<IrrigationActionDto> CancelAsync(Guid irrigationActionId, CancelIrrigationDto dto, CancellationToken cancellationToken = default);

    Task<IrrigationActionDto> UpdateCancellationReasonAsync(Guid irrigationActionId, UpdateCancellationReasonDto dto, CancellationToken cancellationToken = default);

    Task<IrrigationActionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IrrigationActionDto>> GetRecentByDeviceAsync(string deviceId, int limit, CancellationToken cancellationToken = default);

    Task<IrrigationActionDto?> HandleDeviceAckAsync(IrrigationAckMessage ack, CancellationToken cancellationToken = default);
}
