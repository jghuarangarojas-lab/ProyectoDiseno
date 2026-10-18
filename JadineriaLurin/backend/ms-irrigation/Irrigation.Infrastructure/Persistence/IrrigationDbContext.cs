using Irrigation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Irrigation.Infrastructure.Persistence;

public class IrrigationDbContext : DbContext
{
    public IrrigationDbContext(DbContextOptions<IrrigationDbContext> options)
        : base(options)
    {
    }

    public DbSet<IrrigationAction> IrrigationActions => Set<IrrigationAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var irrigationAction = modelBuilder.Entity<IrrigationAction>();

        irrigationAction.ToTable("irrigation_actions");
        irrigationAction.HasKey(action => action.Id)
            .HasName("pk_irrigation_actions");

        irrigationAction.Property(action => action.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        irrigationAction.Property(action => action.DeviceId)
            .HasColumnName("device_id")
            .HasMaxLength(64)
            .IsRequired();

        irrigationAction.Property(action => action.SectorArea)
            .HasColumnName("sector_area")
            .HasMaxLength(32)
            .IsRequired();

        irrigationAction.Property(action => action.TriggerType)
            .HasColumnName("trigger_type")
            .HasMaxLength(16)
            .IsRequired()
            .HasConversion<string>();

        irrigationAction.Property(action => action.RequestedDurationSeconds)
            .HasColumnName("requested_duration_seconds")
            .IsRequired();

        irrigationAction.Property(action => action.MaxPumpDurationSeconds)
            .HasColumnName("max_pump_duration_seconds")
            .IsRequired();

        irrigationAction.Property(action => action.ActualDurationSeconds)
            .HasColumnName("actual_duration_seconds");

        irrigationAction.Property(action => action.Status)
            .HasColumnName("status")
            .HasMaxLength(16)
            .IsRequired()
            .HasConversion<string>();

        irrigationAction.Property(action => action.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(256);

        irrigationAction.Property(action => action.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(256);

        irrigationAction.Property(action => action.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        irrigationAction.Property(action => action.StartedAtUtc)
            .HasColumnName("started_at_utc");

        irrigationAction.Property(action => action.FinishedAtUtc)
            .HasColumnName("finished_at_utc");

        irrigationAction.Property(action => action.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        irrigationAction.HasIndex(action => action.DeviceId)
            .HasDatabaseName("ix_irrigation_actions_device_id");

        irrigationAction.HasIndex(action => action.Status)
            .HasDatabaseName("ix_irrigation_actions_status");

        irrigationAction.HasIndex(action => action.CreatedAtUtc)
            .HasDatabaseName("ix_irrigation_actions_created_at_utc");

        irrigationAction.HasIndex(action => new { action.DeviceId, action.Status })
            .HasDatabaseName("ix_irrigation_actions_device_id_status");
    }
}
