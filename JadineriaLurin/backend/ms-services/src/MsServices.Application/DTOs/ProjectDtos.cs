using System.ComponentModel.DataAnnotations;

namespace MsServices.Application.DTOs;

public sealed class CreateProjectRequest
{
    [Required]
    public Guid ClientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal TotalBudget { get; set; }

    [MaxLength(300)]
    public string? SiteAddress { get; set; }
}

public sealed class UpdateProjectRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public Guid? ClientId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TotalBudget { get; set; }

    [MaxLength(300)]
    public string? SiteAddress { get; set; }
}

public sealed class ChangeStageRequest
{
    [Required]
    [MaxLength(30)]
    public string Stage { get; set; } = null!;
}

public sealed class ProjectDto
{
    public Guid ProjectId { get; init; }
    public Guid ClientId { get; init; }
    public string Title { get; init; } = null!;
    public string? SiteAddress { get; init; }
    public string Stage { get; init; } = null!;
    public decimal TotalBudget { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<AttachmentDto> Attachments { get; init; } = Array.Empty<AttachmentDto>();
    public IReadOnlyList<AppointmentDto> Appointments { get; init; } = Array.Empty<AppointmentDto>();
    public IReadOnlyList<InstallmentDto> Installments { get; init; } = Array.Empty<InstallmentDto>();
    public ContractDto? Contract { get; init; }
}

public sealed class ProjectSummaryDto
{
    public Guid ProjectId { get; init; }
    public Guid ClientId { get; init; }
    public string Title { get; init; } = null!;
    public string Stage { get; init; } = null!;
    public decimal TotalBudget { get; init; }
    public int AttachmentCount { get; init; }
    public int AppointmentCount { get; init; }
    public int InstallmentCount { get; init; }
    public decimal PendingBillingAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class AttachmentDto
{
    public Guid AttachmentId { get; init; }
    public Guid ProjectId { get; init; }
    public string FileUrl { get; init; } = null!;
    public string FileType { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public string? ContentType { get; init; }
    public long SizeBytes { get; init; }
    public DateTime UploadedAt { get; init; }
}

public sealed class InstallmentDto
{
    public Guid InstallmentId { get; init; }
    public Guid ProjectId { get; init; }
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? PaidAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class ContractDto
{
    public Guid ContractId { get; init; }
    public Guid ProjectId { get; init; }
    public string? GeneratedPdfUrl { get; init; }
    public string? DigitalSignatureUrl { get; init; }
    public string? SignedBy { get; init; }
    public DateTime? SignedAt { get; init; }
    public DateTime GeneratedAt { get; init; }
}

public sealed class CreateInstallmentRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime DueDate { get; set; }
}

public sealed class GenerateInstallmentsRequest
{
    [Required]
    [Range(1, 60)]
    public int Count { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Range(1, 365)]
    public int IntervalDays { get; set; } = 30;

    public bool DistributeBudgetEvenly { get; set; } = true;

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }
}

public sealed class SignContractRequest
{
    [Required]
    [MaxLength(8_000_000)]
    public string SignatureImageBase64 { get; set; } = null!;

    [MaxLength(200)]
    public string? SignedBy { get; set; }
}

public sealed class BillingSyncResultDto
{
    public bool Accepted { get; init; }
    public Guid ProjectId { get; init; }
    public int InstallmentCount { get; init; }
    public decimal TotalAmount { get; init; }
    public int? HttpStatus { get; init; }
    public string? Message { get; init; }
    public DateTime SentAt { get; init; }
}
