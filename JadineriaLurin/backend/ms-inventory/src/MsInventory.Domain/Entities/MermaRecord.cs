using MsInventory.Domain.Enums;
using MsInventory.Domain.Exceptions;

namespace MsInventory.Domain.Entities;

public class MermaRecord
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductPlant? Product { get; set; }
    public decimal QuantityLost { get; set; }
    public string Reason { get; set; } = null!;
    public string? EvidencePhotoUrl { get; set; }
    public ManagerReviewStatus ManagerReviewStatus { get; set; } = ManagerReviewStatus.PendingReview;
    public DateTime CreatedAt { get; set; }

    public void Approve()
    {
        if (ManagerReviewStatus == ManagerReviewStatus.Approved)
            throw new DomainException($"La merma {Id} ya fue aprobada previamente.");

        ManagerReviewStatus = ManagerReviewStatus.Approved;
    }
}
