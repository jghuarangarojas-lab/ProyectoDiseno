using Irrigation.Application.Interfaces.Repositories;
using Irrigation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Irrigation.Infrastructure.Persistence.Repositories;

public sealed class IrrigationRepository : IIrrigationRepository
{
    private readonly IrrigationDbContext _dbContext;

    public IrrigationRepository(IrrigationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(IrrigationAction action, CancellationToken cancellationToken = default)
    {
        _dbContext.IrrigationActions.Add(action);
        return Task.CompletedTask;
    }

    public Task<IrrigationAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.IrrigationActions
            .FirstOrDefaultAsync(action => action.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<IrrigationAction>> ListRecentByDeviceAsync(string deviceId, int limit, CancellationToken cancellationToken = default)
    {
        return await _dbContext.IrrigationActions
            .Where(action => action.DeviceId == deviceId)
            .OrderByDescending(action => action.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
