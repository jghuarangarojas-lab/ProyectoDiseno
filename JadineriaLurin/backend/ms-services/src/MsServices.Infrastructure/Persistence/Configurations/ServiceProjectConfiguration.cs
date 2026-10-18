using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Infrastructure.Persistence.Configurations;

public sealed class ServiceProjectConfiguration : IEntityTypeConfiguration<ServiceProject>
{
    public void Configure(EntityTypeBuilder<ServiceProject> builder)
    {
        builder.ToTable("service_projects");

        builder.HasKey(p => p.ProjectId);
        builder.Property(p => p.ProjectId).HasColumnName("project_id").ValueGeneratedNever();

        builder.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(p => p.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(p => p.SiteAddress).HasColumnName("site_address").HasMaxLength(300);

        builder.Property(p => p.Stage)
            .HasColumnName("stage")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                stage => stage.ToCode(),
                code => code.ToStage());

        builder.Property(p => p.TotalBudget).HasColumnName("total_budget").HasPrecision(18, 2).IsRequired();

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.Stage);

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Project)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Attachments)
            .WithOne(a => a.Project)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Installments)
            .WithOne(i => i.Project)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Contract)
            .WithOne(c => c.Project)
            .HasForeignKey<ContractDocument>(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
