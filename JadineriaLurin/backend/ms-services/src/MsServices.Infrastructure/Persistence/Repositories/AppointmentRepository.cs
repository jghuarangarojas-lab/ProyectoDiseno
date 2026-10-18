using Microsoft.EntityFrameworkCore;
using MsServices.Domain.Abstractions;
using MsServices.Domain.Entities;

namespace MsServices.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly ServicesDbContext _dbContext;

    public AppointmentRepository(ServicesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        _dbContext.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderBy(a => a.ScheduledDate)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default) =>
        _dbContext.Appointments.AddAsync(appointment, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
