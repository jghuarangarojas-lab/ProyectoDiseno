namespace MsServices.Application.DTOs;

public sealed record BillingInstallmentPayload(
    Guid InstallmentId,
    decimal Amount,
    DateTime DueDate,
    string Status,
    DateTime? PaidAt);

public sealed record BillingPayload(
    Guid ProjectId,
    Guid ClientId,
    string ProjectTitle,
    string Stage,
    decimal TotalBudget,
    DateTime SentAt,
    IReadOnlyList<BillingInstallmentPayload> Installments);
