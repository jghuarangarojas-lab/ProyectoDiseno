using MsServices.Application.DTOs;
using MsServices.Application.Interfaces.Services;
using MsServices.Domain.Abstractions;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;

namespace MsServices.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointments;
    private readonly IServiceProjectRepository _projects;

    public AppointmentService(IAppointmentRepository appointments, IServiceProjectRepository projects)
    {
        _appointments = appointments;
        _projects = projects;
    }

    public async Task<AppointmentDto> ScheduleAsync(
        Guid projectId,
        ScheduleAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        if (project.Stage == ProjectStage.COMPLETED)
            throw new DomainException("No se pueden agendar citas sobre un proyecto completado.");

        if (request.ScheduledDate == default)
            throw new DomainException("Debe indicar la fecha programada de la cita.");

        if (request.ScheduledDate <= DateTime.UtcNow)
            throw new DomainException("La cita debe programarse con una fecha futura.");

        var appointment = new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            GardenerId = request.GardenerId,
            ScheduledDate = request.ScheduledDate,
            Status = AppointmentStatus.SCHEDULED,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _appointments.AddAsync(appointment, cancellationToken);
        await _appointments.SaveChangesAsync(cancellationToken);

        return appointment.ToDto();
    }

    public async Task<IReadOnlyList<AppointmentDto>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _appointments.ListByProjectAsync(projectId, cancellationToken);
        return appointments
            .OrderBy(a => a.ScheduledDate)
            .Select(a => a.ToDto())
            .ToList();
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointments.GetByIdAsync(appointmentId, cancellationToken);
        return appointment?.ToDto();
    }

    public async Task<RescheduleAppointmentResultDto> RescheduleAsync(
        Guid appointmentId,
        RescheduleAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.NewScheduledDate == default)
            throw new DomainException("La reprogramación obliga a indicar una nueva fecha para la cita.");

        var appointment = await _appointments.GetByIdAsync(appointmentId, cancellationToken)
            ?? throw new AppointmentNotFoundException(appointmentId);

        var replacement = appointment.RescheduleTo(request.NewScheduledDate, request.Notes, DateTime.UtcNow);

        await _appointments.AddAsync(replacement, cancellationToken);
        await _appointments.SaveChangesAsync(cancellationToken);

        return new RescheduleAppointmentResultDto
        {
            Original = appointment.ToDto(),
            Replacement = replacement.ToDto()
        };
    }

    public async Task<AppointmentDto> CancelAsync(
        Guid appointmentId,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointments.GetByIdAsync(appointmentId, cancellationToken)
            ?? throw new AppointmentNotFoundException(appointmentId);

        appointment.Cancel(request.Reason);
        await _appointments.SaveChangesAsync(cancellationToken);

        return appointment.ToDto();
    }
}
