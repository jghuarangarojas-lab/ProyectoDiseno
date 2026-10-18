namespace MsServices.Domain.Enums;

public static class ServiceEnums
{
    public static string ToCode(this ProjectStage stage) => stage switch
    {
        ProjectStage.REQUESTED => "REQUESTED",
        ProjectStage.TECH_VISIT => "TECH_VISIT",
        ProjectStage.PROPOSAL => "PROPOSAL",
        ProjectStage.APPROVED => "APPROVED",
        ProjectStage.CONTRACTED => "CONTRACTED",
        ProjectStage.IN_PROGRESS => "IN_PROGRESS",
        ProjectStage.COMPLETED => "COMPLETED",
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
    };

    public static ProjectStage ToStage(this string code) => code.Trim().ToUpperInvariant() switch
    {
        "REQUESTED" => ProjectStage.REQUESTED,
        "TECH_VISIT" => ProjectStage.TECH_VISIT,
        "PROPOSAL" => ProjectStage.PROPOSAL,
        "APPROVED" => ProjectStage.APPROVED,
        "CONTRACTED" => ProjectStage.CONTRACTED,
        "IN_PROGRESS" => ProjectStage.IN_PROGRESS,
        "COMPLETED" => ProjectStage.COMPLETED,
        _ => throw new ArgumentException(
            $"Etapa de proyecto inválida: '{code}'. Valores permitidos: REQUESTED, TECH_VISIT, PROPOSAL, APPROVED, CONTRACTED, IN_PROGRESS, COMPLETED.")
    };

    public static string ToCode(this AppointmentStatus status) => status switch
    {
        AppointmentStatus.SCHEDULED => "SCHEDULED",
        AppointmentStatus.RESCHEDULED => "RESCHEDULED",
        AppointmentStatus.CANCELLED => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static AppointmentStatus ToAppointmentStatus(this string code) => code.Trim().ToUpperInvariant() switch
    {
        "SCHEDULED" => AppointmentStatus.SCHEDULED,
        "RESCHEDULED" => AppointmentStatus.RESCHEDULED,
        "CANCELLED" => AppointmentStatus.CANCELLED,
        _ => throw new ArgumentException(
            $"Estado de cita inválido: '{code}'. Valores permitidos: SCHEDULED, RESCHEDULED, CANCELLED.")
    };

    public static string ToCode(this AttachmentFileType fileType) => fileType switch
    {
        AttachmentFileType.RENDER => "RENDER",
        AttachmentFileType.TWO_D_PLAN => "2D_PLAN",
        AttachmentFileType.INVOICE => "INVOICE",
        _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
    };

    public static AttachmentFileType ToFileType(this string code) => code.Trim().ToUpperInvariant() switch
    {
        "RENDER" => AttachmentFileType.RENDER,
        "2D_PLAN" => AttachmentFileType.TWO_D_PLAN,
        "2DPLAN" => AttachmentFileType.TWO_D_PLAN,
        "INVOICE" => AttachmentFileType.INVOICE,
        _ => throw new ArgumentException(
            $"Tipo de archivo inválido: '{code}'. Valores permitidos: RENDER, 2D_PLAN, INVOICE.")
    };

    public static string ToCode(this InstallmentStatus status) => status switch
    {
        InstallmentStatus.PENDING => "PENDING",
        InstallmentStatus.PAID => "PAID",
        InstallmentStatus.OVERDUE => "OVERDUE",
        InstallmentStatus.CANCELLED => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static InstallmentStatus ToInstallmentStatus(this string code) => code.Trim().ToUpperInvariant() switch
    {
        "PENDING" => InstallmentStatus.PENDING,
        "PAID" => InstallmentStatus.PAID,
        "OVERDUE" => InstallmentStatus.OVERDUE,
        "CANCELLED" => InstallmentStatus.CANCELLED,
        _ => throw new ArgumentException(
            $"Estado de cuota inválido: '{code}'. Valores permitidos: PENDING, PAID, OVERDUE, CANCELLED.")
    };
}
