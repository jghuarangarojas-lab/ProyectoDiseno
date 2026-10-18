using Irrigation.Api.Hubs;
using Irrigation.Application.DTOs.Responses;
using Irrigation.Application.Interfaces.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Irrigation.Api.Realtime;

public sealed class IrrigationHubNotifier : IIrrigationStateNotifier
{
    public const string UpdateMethodName = "irrigationUpdated";

    private readonly IHubContext<IrrigationHub> _hubContext;
    private readonly ILogger<IrrigationHubNotifier> _logger;

    public IrrigationHubNotifier(IHubContext<IrrigationHub> hubContext, ILogger<IrrigationHubNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyStateChangedAsync(IrrigationActionDto action, CancellationToken cancellationToken = default)
    {
        var groups = new[]
        {
            IrrigationHub.DeviceGroup(action.DeviceId),
            IrrigationHub.DashboardGroup
        };

        await _hubContext.Clients.Groups(groups).SendAsync(UpdateMethodName, action, cancellationToken);

        _logger.LogDebug(
            "Actualización de la acción {ActionId} emitida a los grupos {Groups}",
            action.Id,
            string.Join(", ", groups));
    }
}
