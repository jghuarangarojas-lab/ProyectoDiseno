using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;

namespace MsDelivery.Infrastructure.Persistence.Configurations;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.HasIndex(d => d.Name);

        builder.Property(d => d.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();

        builder.Property(d => d.VehicleType)
            .HasColumnName("vehicle_type")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                vehicleType => vehicleType.ToCode(),
                code => VehicleTypeExtensions.FromCode(code));

        builder.Property(d => d.MaxCapacity)
            .HasColumnName("max_capacity")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                capacity => capacity.ToCode(),
                code => CapacityLevelExtensions.FromCode(code));

        builder.Property(d => d.IsActive).HasColumnName("is_active").IsRequired();

        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(d => d.IsActive);

        builder.HasMany(d => d.Orders)
            .WithOne(o => o.Driver)
            .HasForeignKey(o => o.DriverId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
