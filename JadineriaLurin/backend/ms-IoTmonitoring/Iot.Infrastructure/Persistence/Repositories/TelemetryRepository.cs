using Iot.Application.Interfaces.Repositories;
using Iot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Iot.Infrastructure.Persistence.Repositories;

public sealed class TelemetryRepository : ITelemetryRepository
{
    private readonly IotDbContext _dbContext;

    public TelemetryRepository(IotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(TelemetryRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.TelemetryRecords.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<TelemetryRecord>> GetLastAsync(
        string deviceCode,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TelemetryRecords
            .Where(record => record.DeviceCode == deviceCode)
            .OrderByDescending(record => record.RecordedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
