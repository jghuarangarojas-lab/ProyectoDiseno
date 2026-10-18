using MsServices.Application.DTOs;

namespace MsServices.Application.Interfaces.Services;

public interface IAppointmentService
{
    Task<AppointmentDto> ScheduleAsync(Guid projectId, ScheduleAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDto>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<AppointmentDto?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<RescheduleAppointmentResultDto> RescheduleAsync(
        Guid appointmentId,
        RescheduleAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto> CancelAsync(Guid appointmentId, CancelAppointmentRequest request, CancellationToken cancellationToken = default);
}
