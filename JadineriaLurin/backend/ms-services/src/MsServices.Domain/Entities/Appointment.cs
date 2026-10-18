using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;

namespace MsServices.Domain.Entities;

public class Appointment
{
    public Guid AppointmentId { get; set; }
    public Guid ProjectId { get; set; }
    public ServiceProject? Project { get; set; }
    public Guid GardenerId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.SCHEDULED;
    public string? Notes { get; set; }
    public Guid? RescheduledFromId { get; set; }
    public DateTime? RescheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Reprograma la cita: la actual pasa a RESCHEDULED y se devuelve la cita nueva
    /// (SCHEDULED) que obligatoriamente debe tener una fecha diferente.
    /// </summary>
    public Appointment RescheduleTo(DateTime newScheduledDate, string? notes, DateTime now)
    {
        if (Status != AppointmentStatus.SCHEDULED)
            throw new DomainException(
                $"Solo se pueden reprogramar citas con estado SCHEDULED. Estado actual: {Status.ToCode()}.");

        if (newScheduledDate == default)
            throw new DomainException("La reprogramación obliga a indicar una nueva fecha para la cita.");

        if (newScheduledDate <= now)
            throw new DomainException("La nueva fecha de la cita debe ser posterior a la fecha actual.");

        if (newScheduledDate == ScheduledDate)
            throw new DomainException(
                $"La nueva fecha debe ser distinta a la programada actualmente ({ScheduledDate:yyyy-MM-dd HH:mm}).");

        Status = AppointmentStatus.RESCHEDULED;
        RescheduledAt = now;

        return new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ProjectId = ProjectId,
            GardenerId = GardenerId,
            ScheduledDate = newScheduledDate,
            Status = AppointmentStatus.SCHEDULED,
            Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes,
            RescheduledFromId = AppointmentId,
            CreatedAt = now
        };
    }

    public void Cancel(string? reason)
    {
        if (Status == AppointmentStatus.CANCELLED)
            throw new DomainException("La cita ya fue cancelada.");

        Status = AppointmentStatus.CANCELLED;
        if (!string.IsNullOrWhiteSpace(reason))
            Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelación: {reason}";
    }
}
