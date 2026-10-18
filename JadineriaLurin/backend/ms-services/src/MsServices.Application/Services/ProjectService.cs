using MsServices.Application.Abstractions;
using MsServices.Application.DTOs;
using MsServices.Application.Interfaces.External;
using MsServices.Application.Interfaces.Services;
using MsServices.Domain.Abstractions;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;

namespace MsServices.Application.Services;

public sealed class ProjectService : IProjectService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxAttachmentBytes = 20 * 1024 * 1024;
    private const long MaxSignatureBytes = 2 * 1024 * 1024;

    private readonly IServiceProjectRepository _projects;
    private readonly IFileStorageService _fileStorage;
    private readonly IContractPdfService _contractPdf;
    private readonly IBillingIntegration _billingIntegration;

    public ProjectService(
        IServiceProjectRepository projects,
        IFileStorageService fileStorage,
        IContractPdfService contractPdf,
        IBillingIntegration billingIntegration)
    {
        _projects = projects;
        _fileStorage = fileStorage;
        _contractPdf = contractPdf;
        _billingIntegration = billingIntegration;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TotalBudget < 0)
            throw new DomainException("El presupuesto total no puede ser negativo.");

        var now = DateTime.UtcNow;
        var project = new ServiceProject
        {
            ProjectId = Guid.NewGuid(),
            ClientId = request.ClientId,
            Title = request.Title.Trim(),
            SiteAddress = request.SiteAddress?.Trim(),
            Stage = ProjectStage.REQUESTED,
            TotalBudget = request.TotalBudget,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _projects.AddAsync(project, cancellationToken);
        await _projects.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        return project?.ToDto();
    }

    public async Task<(IReadOnlyList<ProjectSummaryDto> Items, int TotalCount)> ListAsync(
        ProjectStage? stage,
        Guid? clientId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _projects.CountAsync(stage, clientId, cancellationToken);
        var items = await _projects.ListAsync(stage, clientId, null, (page - 1) * pageSize, pageSize, cancellationToken);

        return (items.Select(p => p.ToSummaryDto()).ToList(), totalCount);
    }

    public async Task<ProjectDto> UpdateAsync(
        Guid projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);

        if (request.TotalBudget < 0)
            throw new DomainException("El presupuesto total no puede ser negativo.");

        if (!string.IsNullOrWhiteSpace(request.Title))
            project.Title = request.Title.Trim();

        if (request.ClientId.HasValue)
            project.ClientId = request.ClientId.Value;

        if (request.TotalBudget.HasValue)
            project.TotalBudget = request.TotalBudget.Value;

        if (request.SiteAddress is not null)
            project.SiteAddress = request.SiteAddress.Trim();

        project.Touch(DateTime.UtcNow);
        await _projects.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }

    public async Task<ProjectDto> ChangeStageAsync(
        Guid projectId,
        string stage,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        project.ChangeStage(stage.ToStage(), DateTime.UtcNow);

        await _projects.SaveChangesAsync(cancellationToken);
        return project.ToDto();
    }

    public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        _projects.Remove(project);
        await _projects.SaveChangesAsync(cancellationToken);
    }

    public async Task<AttachmentDto> AddAttachmentAsync(
        Guid projectId,
        string fileType,
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        var type = fileType.ToFileType();

        if (content.Length == 0)
            throw new DomainException("El archivo adjunto está vacío.");

        if (content.Length > MaxAttachmentBytes)
            throw new DomainException("El archivo adjunto supera el máximo permitido de 20 MB.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var isInvoice = type == AttachmentFileType.INVOICE;

        if (isInvoice && extension != ".pdf")
            throw new DomainException("Los adjuntos de tipo INVOICE deben ser archivos PDF (.pdf).");

        if (!isInvoice && !ImageExtensions.Contains(extension))
            throw new DomainException(
                $"Los adjuntos de tipo {type.ToCode()} deben ser imágenes ({string.Join(", ", ImageExtensions)}).");

        var url = await _fileStorage.SaveAsync(content, fileName, "projects/attachments", cancellationToken);

        var attachment = new ProjectAttachment
        {
            AttachmentId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            FileUrl = url,
            FileType = type,
            FileName = Path.GetFileName(fileName),
            ContentType = contentType,
            SizeBytes = content.Length,
            UploadedAt = DateTime.UtcNow
        };

        project.Attachments.Add(attachment);
        project.Touch(DateTime.UtcNow);
        await _projects.SaveChangesAsync(cancellationToken);

        return attachment.ToDto();
    }

    public async Task RemoveAttachmentAsync(
        Guid projectId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        var attachment = project.Attachments.FirstOrDefault(a => a.AttachmentId == attachmentId)
            ?? throw new DomainException($"El archivo adjunto {attachmentId} no pertenece al proyecto {projectId}.");

        project.Attachments.Remove(attachment);
        project.Touch(DateTime.UtcNow);
        await _projects.SaveChangesAsync(cancellationToken);

        try
        {
            await _fileStorage.DeleteAsync(attachment.FileUrl, cancellationToken);
        }
        catch (IOException)
        {
            // El registro ya fue eliminado; un archivo huérfano no debe romper la operación.
        }
    }

    public async Task<IReadOnlyList<InstallmentDto>> ListInstallmentsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var installment in project.Installments)
            installment.RefreshOverdueState(now);

        return project.Installments
            .OrderBy(i => i.DueDate)
            .Select(i => i.ToDto())
            .ToList();
    }

    public async Task<InstallmentDto> AddInstallmentAsync(
        Guid projectId,
        CreateInstallmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        var installment = project.AddInstallment(request.Amount, request.DueDate, DateTime.UtcNow);

        await _projects.SaveChangesAsync(cancellationToken);
        return installment.ToDto();
    }

    public async Task<IReadOnlyList<InstallmentDto>> GenerateInstallmentsAsync(
        Guid projectId,
        GenerateInstallmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        var now = DateTime.UtcNow;

        decimal amountPerInstallment;
        if (request.DistributeBudgetEvenly)
        {
            if (project.TotalBudget <= 0)
                throw new DomainException(
                    "El proyecto no tiene presupuesto registrado; no se pueden distribuir las cuotas. " +
                    "Use un monto fijo por cuota o defina el presupuesto total del proyecto.");

            amountPerInstallment = Math.Round(
                project.TotalBudget / request.Count, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            if (request.Amount is null or <= 0)
                throw new DomainException(
                    "Debe indicar el monto fijo de cada cuota o activar la distribución del presupuesto total.");

            amountPerInstallment = request.Amount.Value;
        }

        var created = new List<PaymentInstallment>(request.Count);

        for (var index = 0; index < request.Count; index++)
        {
            var amount = amountPerInstallment;
            if (request.DistributeBudgetEvenly && index == request.Count - 1)
            {
                var distributed = amountPerInstallment * request.Count;
                var roundingAdjustment = Math.Round(project.TotalBudget - distributed, 2, MidpointRounding.AwayFromZero);
                amount = Math.Round(amountPerInstallment + roundingAdjustment, 2, MidpointRounding.AwayFromZero);
            }

            var dueDate = request.StartDate.AddDays(request.IntervalDays * index);
            var installment = project.AddInstallment(amount, dueDate, now);
            created.Add(installment);
        }

        await _projects.SaveChangesAsync(cancellationToken);
        return created.Select(i => i.ToDto()).ToList();
    }

    public async Task<InstallmentDto> MarkInstallmentPaidAsync(
        Guid projectId,
        Guid installmentId,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        var installment = project.Installments.FirstOrDefault(i => i.InstallmentId == installmentId)
            ?? throw new DomainException($"La cuota {installmentId} no pertenece al proyecto {projectId}.");

        installment.MarkPaid(DateTime.UtcNow);
        project.Touch(DateTime.UtcNow);

        await _projects.SaveChangesAsync(cancellationToken);
        return installment.ToDto();
    }

    public async Task<ContractDto> GenerateContractAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);

        if (project.TotalBudget <= 0)
            throw new DomainException(
                "No se puede generar el contrato sin presupuesto total definido en el proyecto.");

        var now = DateTime.UtcNow;
        var contract = project.Contract;
        if (contract is null)
        {
            contract = new ContractDocument
            {
                ContractId = Guid.NewGuid(),
                ProjectId = project.ProjectId
            };
            project.Contract = contract;
        }

        var pdfBytes = await _contractPdf.GenerateAsync(project, project.Installments, cancellationToken);

        await using var pdfStream = new MemoryStream(pdfBytes);
        var fileUrl = await _fileStorage.SaveAsync(
            pdfStream,
            $"contrato-{project.ProjectId:N}.pdf",
            "projects/contracts",
            cancellationToken);

        contract.AttachGeneratedPdf(fileUrl, now);
        project.Touch(now);

        await _projects.SaveChangesAsync(cancellationToken);
        return contract.ToDto();
    }

    public async Task<ContractDto> SignContractAsync(
        Guid projectId,
        SignContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);
        project.EnsureCanAdvanceFrom(ProjectStage.APPROVED, ProjectStage.CONTRACTED);

        var now = DateTime.UtcNow;
        var (signatureBytes, extension) = DecodeSignature(request.SignatureImageBase64);

        await using var signatureStream = new MemoryStream(signatureBytes);
        var fileUrl = await _fileStorage.SaveAsync(
            signatureStream,
            $"firma-{project.ProjectId:N}{extension}",
            "projects/signatures",
            cancellationToken);

        var contract = project.Contract;
        if (contract is null)
        {
            contract = new ContractDocument
            {
                ContractId = Guid.NewGuid(),
                ProjectId = project.ProjectId
            };
            project.Contract = contract;
        }

        contract.Sign(fileUrl, request.SignedBy, now);

        if (project.Stage == ProjectStage.APPROVED)
            project.ChangeStage(ProjectStage.CONTRACTED, now);

        project.Touch(now);
        await _projects.SaveChangesAsync(cancellationToken);

        return contract.ToDto();
    }

    public async Task<BillingSyncResultDto> SyncBillingAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await RequireAsync(projectId, cancellationToken);

        if (project.Installments.Count == 0)
            throw new DomainException(
                "El proyecto no tiene cuotas de pago registradas; no hay nada que sincronizar con caja.");

        var installments = project.Installments.OrderBy(i => i.DueDate).ToList();
        return await _billingIntegration.SendInstallmentsAsync(project, installments, cancellationToken);
    }

    private async Task<ServiceProject> RequireAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _projects.GetByIdAsync(projectId, cancellationToken)
        ?? throw new ProjectNotFoundException(projectId);

    private static (byte[] Bytes, string Extension) DecodeSignature(string rawValue)
    {
        var value = rawValue.Trim();
        var contentType = "image/png";
        var extension = ".png";

        var dataUriSeparator = value.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && dataUriSeparator > 0)
        {
            var header = value[..value.IndexOf(',', StringComparison.Ordinal)];
            value = value[(dataUriSeparator + "base64,".Length)..];
            contentType = header[5..header.IndexOf(';')];
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/webp" => ".webp",
                _ => ".png"
            };
        }

        value = value.Replace("\r", string.Empty).Replace("\n", string.Empty);

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            throw new DomainException("La firma enviada no es una imagen Base64 válida.");
        }

        if (bytes.Length == 0)
            throw new DomainException("La imagen de la firma está vacía.");

        if (bytes.Length > MaxSignatureBytes)
            throw new DomainException("La imagen de la firma supera el máximo permitido de 2 MB.");

        return (bytes, extension);
    }
}
