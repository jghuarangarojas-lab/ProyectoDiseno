using Iot.Application.Interfaces.Repositories;
using Iot.Domain.Entities;
using Iot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Iot.Infrastructure.Persistence.Repositories;

public sealed class DeviceAlertRepository : IDeviceAlertRepository
{
    private readonly IotDbContext _dbContext;

    public DeviceAlertRepository(IotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(DeviceAlert alert, CancellationToken cancellationToken = default)
    {
        _dbContext.DeviceAlerts.Add(alert);
        return Task.CompletedTask;
    }

    public Task<DeviceAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.DeviceAlerts
            .FirstOrDefaultAsync(alert => alert.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceAlert>> GetAllAsync(
        string? deviceCode,
        bool includeResolved,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DeviceAlerts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(deviceCode))
        {
            query = query.Where(alert => alert.DeviceCode == deviceCode);
        }

        if (!includeResolved)
        {
            query = query.Where(alert => !alert.IsResolved);
        }

        return await query
            .OrderByDescending(alert => alert.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsUnresolvedAsync(
        string deviceCode,
        AlertIssueType issueType,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DeviceAlerts
            .AnyAsync(alert =>
                alert.DeviceCode == deviceCode &&
                alert.IssueType == issueType &&
                !alert.IsResolved,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
