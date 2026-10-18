using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;

namespace MsServices.Domain.Entities;

public class PaymentInstallment
{
    public Guid InstallmentId { get; set; }
    public Guid ProjectId { get; set; }
    public ServiceProject? Project { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.PENDING;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public void MarkPaid(DateTime now)
    {
        if (Status == InstallmentStatus.PAID)
            throw new DomainException($"La cuota {InstallmentId} ya fue pagada.");

        Status = InstallmentStatus.PAID;
        PaidAt = now;
    }

    public void Cancel()
    {
        if (Status == InstallmentStatus.PAID)
            throw new DomainException("No se puede anular una cuota ya pagada.");

        Status = InstallmentStatus.CANCELLED;
    }

    public void RefreshOverdueState(DateTime now)
    {
        if (Status == InstallmentStatus.PENDING && DueDate.Date < now.Date)
            Status = InstallmentStatus.OVERDUE;
    }
}
