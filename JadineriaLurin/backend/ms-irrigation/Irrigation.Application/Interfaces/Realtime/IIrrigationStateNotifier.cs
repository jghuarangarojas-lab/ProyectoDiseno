using Irrigation.Application.DTOs.Responses;

namespace Irrigation.Application.Interfaces.Realtime;

public interface IIrrigationStateNotifier
{
    Task NotifyStateChangedAsync(IrrigationActionDto action, CancellationToken cancellationToken = default);
}
