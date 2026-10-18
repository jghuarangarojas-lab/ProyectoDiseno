using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Domain.Abstractions;

public interface IServiceProjectRepository
{
    Task<ServiceProject?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceProject>> ListAsync(
        ProjectStage? stage,
        Guid? clientId,
        bool? onlyWithPendingBilling,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(ProjectStage? stage, Guid? clientId, CancellationToken cancellationToken = default);

    Task AddAsync(ServiceProject project, CancellationToken cancellationToken = default);

    void Remove(ServiceProject project);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
