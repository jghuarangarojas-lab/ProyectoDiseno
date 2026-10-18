using Microsoft.EntityFrameworkCore;
using MsDelivery.Domain.Abstractions;
using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;
using MsDelivery.Infrastructure.Persistence;

namespace MsDelivery.Infrastructure.Persistence.Repositories;

public sealed class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _dbContext;

    public DeliveryRepository(DeliveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DeliveryOrder?> GetOrderByIdAsync(
        Guid id,
        bool includeDriver = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DeliveryOrders.AsQueryable();

        if (includeDriver)
            query = query.Include(o => o.Driver);

        return query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<DeliveryOrder?> GetOrderBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default) =>
        _dbContext.DeliveryOrders
            .FirstOrDefaultAsync(o => o.SaleId == saleId, cancellationToken);

    public async Task<IReadOnlyList<DeliveryOrder>> GetOrdersAsync(
        DeliveryStatus? status = null,
        Guid? driverId = null,
        DateOnly? deliveryDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOrdersQuery(status, driverId, deliveryDate);

        return await query
            .AsNoTracking()
            .OrderBy(o => o.DeliveryDate)
            .ThenBy(o => o.DeliveryTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountOrdersAsync(
        DeliveryStatus? status = null,
        Guid? driverId = null,
        DateOnly? deliveryDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOrdersQuery(status, driverId, deliveryDate);
        return await query.AsNoTracking().CountAsync(cancellationToken);
    }

    public Task AddOrderAsync(DeliveryOrder order, CancellationToken cancellationToken = default) =>
        _dbContext.DeliveryOrders.AddAsync(order, cancellationToken).AsTask();

    public Task<Driver?> GetDriverByIdAsync(
        Guid id,
        bool includeOrders = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Drivers.AsQueryable();

        if (includeOrders)
            query = query.Include(d => d.Orders);

        return query.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Driver>> GetDriversAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Drivers.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(d => d.IsActive == isActive.Value);

        return await query
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddDriverAsync(Driver driver, CancellationToken cancellationToken = default) =>
        _dbContext.Drivers.AddAsync(driver, cancellationToken).AsTask();

    public async Task<IReadOnlyList<DeliveryOrder>> GetActiveOrdersByDriverIdsAsync(
        IReadOnlyCollection<Guid> driverIds,
        CancellationToken cancellationToken = default)
    {
        if (driverIds.Count == 0)
            return Array.Empty<DeliveryOrder>();

        return await _dbContext.DeliveryOrders
            .AsNoTracking()
            .Where(o => o.DriverId.HasValue
                && driverIds.Contains(o.DriverId.Value)
                && (o.Status == DeliveryStatus.Pending || o.Status == DeliveryStatus.Dispatched))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<DeliveryOrder> BuildOrdersQuery(
        DeliveryStatus? status,
        Guid? driverId,
        DateOnly? deliveryDate)
    {
        var query = _dbContext.DeliveryOrders.AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (driverId.HasValue)
            query = query.Where(o => o.DriverId == driverId.Value);

        if (deliveryDate.HasValue)
            query = query.Where(o => o.DeliveryDate == deliveryDate.Value);

        return query;
    }
}
