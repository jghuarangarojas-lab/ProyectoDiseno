using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Application.DTOs;

public static class DtoMapper
{
    public static ProjectDto ToDto(this ServiceProject project) => new()
    {
        ProjectId = project.ProjectId,
        ClientId = project.ClientId,
        Title = project.Title,
        SiteAddress = project.SiteAddress,
        Stage = project.Stage.ToCode(),
        TotalBudget = project.TotalBudget,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
        Attachments = project.Attachments
            .OrderBy(a => a.UploadedAt)
            .Select(ToDto)
            .ToList(),
        Appointments = project.Appointments
            .OrderBy(a => a.ScheduledDate)
            .Select(ToDto)
            .ToList(),
        Installments = project.Installments
            .OrderBy(i => i.DueDate)
            .Select(ToDto)
            .ToList(),
        Contract = project.Contract?.ToDto()
    };

    public static ProjectSummaryDto ToSummaryDto(this ServiceProject project) => new()
    {
        ProjectId = project.ProjectId,
        ClientId = project.ClientId,
        Title = project.Title,
        Stage = project.Stage.ToCode(),
        TotalBudget = project.TotalBudget,
        AttachmentCount = project.Attachments.Count,
        AppointmentCount = project.Appointments.Count,
        InstallmentCount = project.Installments.Count,
        PendingBillingAmount = project.Installments
            .Where(i => i.Status is InstallmentStatus.PENDING or InstallmentStatus.OVERDUE)
            .Sum(i => i.Amount),
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt
    };

    public static AttachmentDto ToDto(this ProjectAttachment attachment) => new()
    {
        AttachmentId = attachment.AttachmentId,
        ProjectId = attachment.ProjectId,
        FileUrl = attachment.FileUrl,
        FileType = attachment.FileType.ToCode(),
        FileName = attachment.FileName,
        ContentType = attachment.ContentType,
        SizeBytes = attachment.SizeBytes,
        UploadedAt = attachment.UploadedAt
    };

    public static InstallmentDto ToDto(this PaymentInstallment installment) => new()
    {
        InstallmentId = installment.InstallmentId,
        ProjectId = installment.ProjectId,
        Amount = installment.Amount,
        DueDate = installment.DueDate,
        Status = installment.Status.ToCode(),
        PaidAt = installment.PaidAt,
        CreatedAt = installment.CreatedAt
    };

    public static ContractDto ToDto(this ContractDocument contract) => new()
    {
        ContractId = contract.ContractId,
        ProjectId = contract.ProjectId,
        GeneratedPdfUrl = contract.GeneratedPdfUrl,
        DigitalSignatureUrl = contract.DigitalSignatureUrl,
        SignedBy = contract.SignedBy,
        SignedAt = contract.SignedAt,
        GeneratedAt = contract.GeneratedAt
    };

    public static AppointmentDto ToDto(this Appointment appointment) => new()
    {
        AppointmentId = appointment.AppointmentId,
        ProjectId = appointment.ProjectId,
        GardenerId = appointment.GardenerId,
        ScheduledDate = appointment.ScheduledDate,
        Status = appointment.Status.ToCode(),
        Notes = appointment.Notes,
        RescheduledFromId = appointment.RescheduledFromId,
        RescheduledAt = appointment.RescheduledAt,
        CreatedAt = appointment.CreatedAt
    };
}
