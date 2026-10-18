using Iot.Domain.Entities;
using Iot.Domain.Enums;

namespace Iot.Application.Interfaces.Repositories;

public interface IDeviceAlertRepository
{
    Task AddAsync(DeviceAlert alert, CancellationToken cancellationToken = default);

    Task<DeviceAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceAlert>> GetAllAsync(
        string? deviceCode,
        bool includeResolved,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsUnresolvedAsync(
        string deviceCode,
        AlertIssueType issueType,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
