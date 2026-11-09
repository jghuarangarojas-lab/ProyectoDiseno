using Iot.Domain.Entities;

namespace Iot.Application.Interfaces.Repositories;

public interface ITelemetryRepository
{
    Task AddAsync(TelemetryRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelemetryRecord>> GetLastAsync(string deviceCode, int count, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
