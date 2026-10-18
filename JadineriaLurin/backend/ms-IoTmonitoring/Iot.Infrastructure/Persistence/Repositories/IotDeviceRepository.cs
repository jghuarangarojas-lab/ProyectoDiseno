using Iot.Application.Interfaces.Repositories;
using Iot.Domain.Entities;
using Iot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Iot.Infrastructure.Persistence.Repositories;

public sealed class IotDeviceRepository : IIotDeviceRepository
{
    private readonly IotDbContext _dbContext;

    public IotDeviceRepository(IotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(IotDevice device, CancellationToken cancellationToken = default)
    {
        _dbContext.IotDevices.Add(device);
        return Task.CompletedTask;
    }

    public Task<IotDevice?> GetByCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.IotDevices
            .Include(device => device.Threshold)
            .FirstOrDefaultAsync(device => device.DeviceCode == deviceCode, cancellationToken);
    }

    public async Task<IReadOnlyList<IotDevice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.IotDevices
            .Include(device => device.Threshold)
            .OrderBy(device => device.DeviceCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IotDevice>> GetStaleAsync(
        DateTime cutoffUtc,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IotDevices
            .Include(device => device.Threshold)
            .Where(device =>
                device.Status == DeviceStatus.ONLINE &&
                device.LastSeenAtUtc != null &&
                device.LastSeenAtUtc < cutoffUtc)
            .OrderBy(device => device.LastSeenAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
