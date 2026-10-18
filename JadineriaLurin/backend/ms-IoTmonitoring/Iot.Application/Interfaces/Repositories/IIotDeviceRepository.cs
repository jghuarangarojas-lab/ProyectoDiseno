using Iot.Domain.Entities;

namespace Iot.Application.Interfaces.Repositories;

public interface IIotDeviceRepository
{
    Task AddAsync(IotDevice device, CancellationToken cancellationToken = default);

    Task<IotDevice?> GetByCodeAsync(string deviceCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IotDevice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IotDevice>> GetStaleAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
