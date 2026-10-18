using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;

namespace MsDelivery.Domain.Abstractions;

public interface IDeliveryRepository
{
    Task<DeliveryOrder?> GetOrderByIdAsync(Guid id, bool includeDriver = false, CancellationToken cancellationToken = default);
    Task<DeliveryOrder?> GetOrderBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryOrder>> GetOrdersAsync(
        DeliveryStatus? status = null,
        Guid? driverId = null,
        DateOnly? deliveryDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);
    Task<int> CountOrdersAsync(
        DeliveryStatus? status = null,
        Guid? driverId = null,
        DateOnly? deliveryDate = null,
        CancellationToken cancellationToken = default);
    Task AddOrderAsync(DeliveryOrder order, CancellationToken cancellationToken = default);

    Task<Driver?> GetDriverByIdAsync(Guid id, bool includeOrders = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Driver>> GetDriversAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task AddDriverAsync(Driver driver, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeliveryOrder>> GetActiveOrdersByDriverIdsAsync(
        IReadOnlyCollection<Guid> driverIds,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
