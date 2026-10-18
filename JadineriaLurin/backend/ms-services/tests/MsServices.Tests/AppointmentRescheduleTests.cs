using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;
using Xunit;

namespace MsServices.Tests;

public class AppointmentRescheduleTests
{
    private static Appointment CreateAppointment(DateTime scheduledDate) => new()
    {
        AppointmentId = Guid.NewGuid(),
        ProjectId = Guid.NewGuid(),
        GardenerId = Guid.NewGuid(),
        ScheduledDate = scheduledDate,
        Status = AppointmentStatus.SCHEDULED,
        Notes = "Visita inicial",
        CreatedAt = DateTime.UtcNow.AddDays(-3)
    };

    [Fact]
    public void Reschedule_MarksOriginalAsRescheduled_AndCreatesNewScheduledAppointment()
    {
        var now = DateTime.UtcNow;
        var original = CreateAppointment(now.AddDays(5));
        var newDate = now.AddDays(8);

        var replacement = original.RescheduleTo(newDate, "Cliente solicitó otro día", now);

        Assert.Equal(AppointmentStatus.RESCHEDULED, original.Status);
        Assert.NotNull(original.RescheduledAt);

        Assert.Equal(AppointmentStatus.SCHEDULED, replacement.Status);
        Assert.Equal(newDate, replacement.ScheduledDate);
        Assert.Equal(original.AppointmentId, replacement.RescheduledFromId);
        Assert.Equal(original.ProjectId, replacement.ProjectId);
        Assert.Equal(original.GardenerId, replacement.GardenerId);
        Assert.NotEqual(original.AppointmentId, replacement.AppointmentId);
        Assert.Contains("Cliente solicitó otro día", replacement.Notes);
    }

    [Fact]
    public void Reschedule_WithoutNewDate_IsRejected()
    {
        var now = DateTime.UtcNow;
        var original = CreateAppointment(now.AddDays(5));

        var exception = Assert.Throws<DomainException>(
            () => original.RescheduleTo(default, null, now));

        Assert.Contains("nueva fecha", exception.Message);
        Assert.Equal(AppointmentStatus.SCHEDULED, original.Status);
    }

    [Fact]
    public void Reschedule_WithSameDate_IsRejected()
    {
        var now = DateTime.UtcNow;
        var original = CreateAppointment(now.AddDays(5));

        Assert.Throws<DomainException>(
            () => original.RescheduleTo(original.ScheduledDate, null, now));

        Assert.Equal(AppointmentStatus.SCHEDULED, original.Status);
    }

    [Fact]
    public void Reschedule_WithPastDate_IsRejected()
    {
        var now = DateTime.UtcNow;
        var original = CreateAppointment(now.AddDays(5));

        Assert.Throws<DomainException>(
            () => original.RescheduleTo(now.AddDays(-1), null, now));
    }

    [Fact]
    public void Reschedule_WhenAlreadyRescheduledOrCancelled_IsRejected()
    {
        var now = DateTime.UtcNow;
        var rescheduled = CreateAppointment(now.AddDays(5));
        rescheduled.Status = AppointmentStatus.RESCHEDULED;

        Assert.Throws<DomainException>(() => rescheduled.RescheduleTo(now.AddDays(6), null, now));

        var cancelled = CreateAppointment(now.AddDays(5));
        cancelled.Status = AppointmentStatus.CANCELLED;

        Assert.Throws<DomainException>(() => cancelled.RescheduleTo(now.AddDays(6), null, now));
    }

    [Fact]
    public void Cancel_OnlyOnce_AndAppendsReason()
    {
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        appointment.Cancel("Lluvia");

        Assert.Equal(AppointmentStatus.CANCELLED, appointment.Status);
        Assert.Contains("Lluvia", appointment.Notes);

        Assert.Throws<DomainException>(() => appointment.Cancel("Otra vez"));
    }
}
