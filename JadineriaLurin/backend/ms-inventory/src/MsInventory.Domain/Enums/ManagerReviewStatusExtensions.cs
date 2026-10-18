namespace MsInventory.Domain.Enums;

public static class ManagerReviewStatusExtensions
{
    public static string ToCode(this ManagerReviewStatus status) => status switch
    {
        ManagerReviewStatus.PendingReview => "PENDING_REVIEW",
        ManagerReviewStatus.Approved => "APPROVED",
        ManagerReviewStatus.Rejected => "REJECTED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static ManagerReviewStatus FromCode(string code) => code.Trim().ToUpperInvariant() switch
    {
        "PENDING_REVIEW" => ManagerReviewStatus.PendingReview,
        "APPROVED" => ManagerReviewStatus.Approved,
        "REJECTED" => ManagerReviewStatus.Rejected,
        _ => throw new ArgumentException($"Estado de revisión inválido: '{code}'. Valores permitidos: PENDING_REVIEW, APPROVED, REJECTED.")
    };
}
