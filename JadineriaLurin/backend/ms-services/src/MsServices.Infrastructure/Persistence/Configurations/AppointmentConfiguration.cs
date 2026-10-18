using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.AppointmentId).HasColumnName("appointment_id").ValueGeneratedNever();

        builder.Property(a => a.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(a => a.GardenerId).HasColumnName("gardener_id").IsRequired();
        builder.Property(a => a.ScheduledDate).HasColumnName("scheduled_date").IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                status => status.ToCode(),
                code => code.ToAppointmentStatus());

        builder.Property(a => a.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(a => a.RescheduledFromId).HasColumnName("rescheduled_from_id");
        builder.Property(a => a.RescheduledAt).HasColumnName("rescheduled_at");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(a => a.ProjectId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ScheduledDate);
    }
}
