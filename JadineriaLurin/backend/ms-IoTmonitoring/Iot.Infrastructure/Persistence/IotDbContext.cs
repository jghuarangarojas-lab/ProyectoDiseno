using Iot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Iot.Infrastructure.Persistence;

public class IotDbContext : DbContext
{
    public IotDbContext(DbContextOptions<IotDbContext> options)
        : base(options)
    {
    }

    public DbSet<IotDevice> IotDevices => Set<IotDevice>();
    public DbSet<BotanicalThreshold> BotanicalThresholds => Set<BotanicalThreshold>();
    public DbSet<TelemetryRecord> TelemetryRecords => Set<TelemetryRecord>();
    public DbSet<DeviceAlert> DeviceAlerts => Set<DeviceAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var device = modelBuilder.Entity<IotDevice>();
        device.ToTable("iot_devices");
        device.HasKey(iotDevice => iotDevice.DeviceCode)
            .HasName("pk_iot_devices");
        device.Property(iotDevice => iotDevice.DeviceCode)
            .HasColumnName("device_code")
            .HasMaxLength(IotDevice.MaxDeviceCodeLength)
            .ValueGeneratedNever();
        device.Property(iotDevice => iotDevice.MacAddress)
            .HasColumnName("mac_address")
            .HasMaxLength(IotDevice.MaxMacAddressLength)
            .IsRequired();
        device.Property(iotDevice => iotDevice.CurrentSectorArea)
            .HasColumnName("current_sector_area")
            .HasMaxLength(IotDevice.MaxSectorAreaLength)
            .IsRequired();
        device.Property(iotDevice => iotDevice.Status)
            .HasColumnName("status")
            .HasMaxLength(16)
            .IsRequired()
            .HasConversion<string>();
        device.Property(iotDevice => iotDevice.BatteryPercentage)
            .HasColumnName("battery_percentage")
            .IsRequired();
        device.Property(iotDevice => iotDevice.LastSeenAtUtc)
            .HasColumnName("last_seen_at_utc");
        device.Property(iotDevice => iotDevice.LastIrrigationRequestedAtUtc)
            .HasColumnName("last_irrigation_requested_at_utc");
        device.Property(iotDevice => iotDevice.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
        device.Property(iotDevice => iotDevice.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();
        device.HasIndex(iotDevice => iotDevice.MacAddress)
            .HasDatabaseName("ix_iot_devices_mac_address");
        device.HasIndex(iotDevice => iotDevice.CurrentSectorArea)
            .HasDatabaseName("ix_iot_devices_current_sector_area");
        device.HasIndex(iotDevice => iotDevice.Status)
            .HasDatabaseName("ix_iot_devices_status");
        device.HasOne(iotDevice => iotDevice.Threshold)
            .WithOne()
            .HasForeignKey<BotanicalThreshold>(threshold => threshold.DeviceCode)
            .OnDelete(DeleteBehavior.Cascade);

        var threshold = modelBuilder.Entity<BotanicalThreshold>();
        threshold.ToTable("botanical_thresholds");
        threshold.HasKey(botanical => botanical.Id)
            .HasName("pk_botanical_thresholds");
        threshold.Property(botanical => botanical.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        threshold.Property(botanical => botanical.DeviceCode)
            .HasColumnName("device_code")
            .HasMaxLength(IotDevice.MaxDeviceCodeLength)
            .IsRequired();
        threshold.Property(botanical => botanical.MinHumidityPercentage)
            .HasColumnName("min_humidity_percentage")
            .IsRequired();
        threshold.Property(botanical => botanical.MaxHumidityPercentage)
            .HasColumnName("max_humidity_percentage")
            .IsRequired();
        threshold.Property(botanical => botanical.MinTemperatureCelsius)
            .HasColumnName("min_temperature_celsius")
            .HasColumnType("double precision")
            .IsRequired();
        threshold.Property(botanical => botanical.MaxTemperatureCelsius)
            .HasColumnName("max_temperature_celsius")
            .HasColumnType("double precision")
            .IsRequired();
        threshold.Property(botanical => botanical.AutoIrrigationSeconds)
            .HasColumnName("auto_irrigation_seconds")
            .IsRequired();
        threshold.Property(botanical => botanical.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        var telemetry = modelBuilder.Entity<TelemetryRecord>();
        telemetry.ToTable("telemetry_records");
        telemetry.HasKey(record => record.Id)
            .HasName("pk_telemetry_records");
        telemetry.Property(record => record.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        telemetry.Property(record => record.DeviceCode)
            .HasColumnName("device_code")
            .HasMaxLength(IotDevice.MaxDeviceCodeLength)
            .IsRequired();
        telemetry.Property(record => record.RecordedAtUtc)
            .HasColumnName("recorded_at_utc")
            .IsRequired();
        telemetry.Property(record => record.HumidityPercentage)
            .HasColumnName("humidity_percentage")
            .HasColumnType("double precision")
            .IsRequired();
        telemetry.Property(record => record.TemperatureCelsius)
            .HasColumnName("temperature_celsius")
            .HasColumnType("double precision")
            .IsRequired();
        telemetry.HasIndex(record => new { record.DeviceCode, record.RecordedAtUtc })
            .HasDatabaseName("ix_telemetry_records_device_code_recorded_at_utc");

        var alert = modelBuilder.Entity<DeviceAlert>();
        alert.ToTable("device_alerts");
        alert.HasKey(deviceAlert => deviceAlert.Id)
            .HasName("pk_device_alerts");
        alert.Property(deviceAlert => deviceAlert.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        alert.Property(deviceAlert => deviceAlert.DeviceCode)
            .HasColumnName("device_code")
            .HasMaxLength(IotDevice.MaxDeviceCodeLength)
            .IsRequired();
        alert.Property(deviceAlert => deviceAlert.IssueType)
            .HasColumnName("issue_type")
            .HasMaxLength(32)
            .IsRequired()
            .HasConversion<string>();
        alert.Property(deviceAlert => deviceAlert.Message)
            .HasColumnName("message")
            .HasMaxLength(DeviceAlert.MaxMessageLength)
            .IsRequired();
        alert.Property(deviceAlert => deviceAlert.IsResolved)
            .HasColumnName("is_resolved")
            .IsRequired();
        alert.Property(deviceAlert => deviceAlert.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
        alert.Property(deviceAlert => deviceAlert.ResolvedAtUtc)
            .HasColumnName("resolved_at_utc");
        alert.Property(deviceAlert => deviceAlert.ResolutionNote)
            .HasColumnName("resolution_note")
            .HasMaxLength(DeviceAlert.MaxResolutionNoteLength);
        alert.HasIndex(deviceAlert => new { deviceAlert.DeviceCode, deviceAlert.IsResolved })
            .HasDatabaseName("ix_device_alerts_device_code_is_resolved");
        alert.HasIndex(deviceAlert => deviceAlert.CreatedAtUtc)
            .HasDatabaseName("ix_device_alerts_created_at_utc");
    }
}
