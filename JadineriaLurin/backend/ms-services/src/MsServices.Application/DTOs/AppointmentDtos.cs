using System.ComponentModel.DataAnnotations;

namespace MsServices.Application.DTOs;

public sealed class ScheduleAppointmentRequest
{
    [Required]
    public Guid GardenerId { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class RescheduleAppointmentRequest
{
    [Required]
    public DateTime NewScheduledDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class CancelAppointmentRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public sealed class AppointmentDto
{
    public Guid AppointmentId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid GardenerId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public string Status { get; init; } = null!;
    public string? Notes { get; init; }
    public Guid? RescheduledFromId { get; init; }
    public DateTime? RescheduledAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class RescheduleAppointmentResultDto
{
    public AppointmentDto Original { get; init; } = null!;
    public AppointmentDto Replacement { get; init; } = null!;
}
