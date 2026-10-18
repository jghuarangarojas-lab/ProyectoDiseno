using Irrigation.Domain.Entities;

namespace Irrigation.Application.Interfaces.Repositories;

public interface IIrrigationRepository
{
    Task AddAsync(IrrigationAction action, CancellationToken cancellationToken = default);

    Task<IrrigationAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IrrigationAction>> ListRecentByDeviceAsync(string deviceId, int limit, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
