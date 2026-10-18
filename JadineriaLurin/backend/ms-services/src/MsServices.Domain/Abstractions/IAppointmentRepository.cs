using MsServices.Domain.Entities;

namespace MsServices.Domain.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
