using Microsoft.EntityFrameworkCore;
using MsServices.Domain.Abstractions;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Infrastructure.Persistence.Repositories;

public sealed class ServiceProjectRepository : IServiceProjectRepository
{
    private readonly ServicesDbContext _dbContext;

    public ServiceProjectRepository(ServicesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ServiceProject?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        _dbContext.ServiceProjects
            .Include(p => p.Attachments)
            .Include(p => p.Appointments)
            .Include(p => p.Installments)
            .Include(p => p.Contract)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);

    public async Task<IReadOnlyList<ServiceProject>> ListAsync(
        ProjectStage? stage,
        Guid? clientId,
        bool? onlyWithPendingBilling,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ServiceProject> query = _dbContext.ServiceProjects
            .AsNoTracking()
            .Include(p => p.Attachments)
            .Include(p => p.Appointments)
            .Include(p => p.Installments)
            .Include(p => p.Contract);

        if (stage.HasValue)
            query = query.Where(p => p.Stage == stage.Value);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        if (onlyWithPendingBilling == true)
            query = query.Where(p => p.Installments.Any(i =>
                i.Status == InstallmentStatus.PENDING || i.Status == InstallmentStatus.OVERDUE));

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ProjectStage? stage, Guid? clientId, CancellationToken cancellationToken = default)
    {
        IQueryable<ServiceProject> query = _dbContext.ServiceProjects.AsNoTracking();

        if (stage.HasValue)
            query = query.Where(p => p.Stage == stage.Value);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        return await query.CountAsync(cancellationToken);
    }

    public Task AddAsync(ServiceProject project, CancellationToken cancellationToken = default) =>
        _dbContext.ServiceProjects.AddAsync(project, cancellationToken).AsTask();

    public void Remove(ServiceProject project) =>
        _dbContext.ServiceProjects.Remove(project);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
