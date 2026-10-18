using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;

namespace MsServices.Domain.Entities;

public class ServiceProject
{
    private static readonly Dictionary<ProjectStage, ProjectStage[]> AllowedTransitions = new()
    {
        [ProjectStage.REQUESTED] = new[] { ProjectStage.TECH_VISIT },
        [ProjectStage.TECH_VISIT] = new[] { ProjectStage.PROPOSAL, ProjectStage.REQUESTED },
        [ProjectStage.PROPOSAL] = new[] { ProjectStage.APPROVED, ProjectStage.TECH_VISIT },
        [ProjectStage.APPROVED] = new[] { ProjectStage.CONTRACTED, ProjectStage.PROPOSAL },
        [ProjectStage.CONTRACTED] = new[] { ProjectStage.IN_PROGRESS, ProjectStage.APPROVED },
        [ProjectStage.IN_PROGRESS] = new[] { ProjectStage.COMPLETED, ProjectStage.CONTRACTED },
        [ProjectStage.COMPLETED] = Array.Empty<ProjectStage>()
    };

    public Guid ProjectId { get; set; }
    public Guid ClientId { get; set; }
    public string Title { get; set; } = null!;
    public string? SiteAddress { get; set; }
    public ProjectStage Stage { get; set; } = ProjectStage.REQUESTED;
    public decimal TotalBudget { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Appointment> Appointments { get; set; } = new();
    public List<ProjectAttachment> Attachments { get; set; } = new();
    public List<PaymentInstallment> Installments { get; set; } = new();
    public ContractDocument? Contract { get; set; }

    public void ChangeStage(ProjectStage target, DateTime now)
    {
        if (target == Stage)
            throw new DomainException($"El proyecto ya se encuentra en la etapa {target.ToCode()}.");

        var allowed = AllowedTransitions[Stage];
        if (!allowed.Contains(target))
            throw new DomainException(
                $"No se puede trasladar el proyecto de {Stage.ToCode()} a {target.ToCode()}. " +
                $"Transiciones permitidas desde {Stage.ToCode()}: {(allowed.Length == 0 ? "(ninguna, etapa final)" : string.Join(", ", allowed.Select(s => s.ToCode())))}.");

        Stage = target;
        Touch(now);
    }

    public void EnsureCanAdvanceFrom(params ProjectStage[] requiredStages)
    {
        if (!requiredStages.Contains(Stage))
            throw new DomainException(
                $"La operación requiere que el proyecto esté en: {string.Join(" o ", requiredStages.Select(s => s.ToCode()))}. " +
                $"Etapa actual: {Stage.ToCode()}.");
    }

    public PaymentInstallment AddInstallment(decimal amount, DateTime dueDate, DateTime now)
    {
        if (amount <= 0)
            throw new DomainException("El monto de la cuota debe ser mayor a cero.");

        if (dueDate == default)
            throw new DomainException("La cuota debe tener una fecha de vencimiento.");

        var installment = new PaymentInstallment
        {
            InstallmentId = Guid.NewGuid(),
            ProjectId = ProjectId,
            Amount = amount,
            DueDate = dueDate,
            Status = InstallmentStatus.PENDING,
            CreatedAt = now
        };

        Installments.Add(installment);
        Touch(now);
        return installment;
    }

    public void Touch(DateTime now) => UpdatedAt = now;
}
