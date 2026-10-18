using MsInventory.Domain.Events;

namespace MsInventory.Application.Abstractions;

public interface ILowStockNotifier
{
    Task NotifyAsync(LowStockDetectedEvent domainEvent, CancellationToken cancellationToken = default);
}
