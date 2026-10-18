using MsServices.Application.DTOs;
using MsServices.Domain.Enums;

namespace MsServices.Application.Interfaces.Services;

public interface IProjectService
{
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ProjectSummaryDto> Items, int TotalCount)> ListAsync(
        ProjectStage? stage,
        Guid? clientId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDto> ChangeStageAsync(Guid projectId, string stage, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<AttachmentDto> AddAttachmentAsync(
        Guid projectId,
        string fileType,
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task RemoveAttachmentAsync(Guid projectId, Guid attachmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InstallmentDto>> ListInstallmentsAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<InstallmentDto> AddInstallmentAsync(Guid projectId, CreateInstallmentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InstallmentDto>> GenerateInstallmentsAsync(
        Guid projectId,
        GenerateInstallmentsRequest request,
        CancellationToken cancellationToken = default);

    Task<InstallmentDto> MarkInstallmentPaidAsync(Guid projectId, Guid installmentId, CancellationToken cancellationToken = default);

    Task<ContractDto> GenerateContractAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ContractDto> SignContractAsync(Guid projectId, SignContractRequest request, CancellationToken cancellationToken = default);

    Task<BillingSyncResultDto> SyncBillingAsync(Guid projectId, CancellationToken cancellationToken = default);
}
