using Microsoft.EntityFrameworkCore;
using MsServices.Domain.Entities;

namespace MsServices.Infrastructure.Persistence;

public class ServicesDbContext : DbContext
{
    public ServicesDbContext(DbContextOptions<ServicesDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceProject> ServiceProjects => Set<ServiceProject>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ProjectAttachment> ProjectAttachments => Set<ProjectAttachment>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServicesDbContext).Assembly);
    }
}
