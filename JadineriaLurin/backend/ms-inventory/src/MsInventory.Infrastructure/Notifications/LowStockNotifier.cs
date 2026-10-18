using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MsInventory.Application.Abstractions;
using MsInventory.Domain.Events;
using MsInventory.Infrastructure.Hubs;

namespace MsInventory.Infrastructure.Notifications;

public sealed class LowStockNotifier : ILowStockNotifier
{
    private readonly IHubContext<InventoryHub> _hubContext;
    private readonly ILogger<LowStockNotifier> _logger;

    public LowStockNotifier(IHubContext<InventoryHub> hubContext, ILogger<LowStockNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyAsync(LowStockDetectedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var payload = new LowStockAlertPayload(
            domainEvent.ProductId,
            domainEvent.Sku,
            domainEvent.CommonName,
            domainEvent.Category,
            domainEvent.StockAvailable,
            domainEvent.StockMinimum,
            domainEvent.SuggestedOrderQuantity,
            domainEvent.DetectedAtUtc);

        await _hubContext.Clients.All.SendAsync(InventoryHubEvents.LowStockAlert, payload, cancellationToken);

        _logger.LogWarning(
            "LowStockAlert emitido para {Sku} ({CommonName}): disponible {StockAvailable} < mínimo {StockMinimum}",
            payload.Sku,
            payload.CommonName,
            payload.StockAvailable,
            payload.StockMinimum);
    }
}
