using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;

namespace MsDelivery.Infrastructure.Persistence.Configurations;

public sealed class DeliveryOrderConfiguration : IEntityTypeConfiguration<DeliveryOrder>
{
    public void Configure(EntityTypeBuilder<DeliveryOrder> builder)
    {
        builder.ToTable("delivery_orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.SaleId).HasColumnName("sale_id").IsRequired();
        builder.HasIndex(o => o.SaleId).IsUnique();

        builder.Property(o => o.Address).HasColumnName("address").HasMaxLength(300).IsRequired();

        builder.Property(o => o.DeliveryDate).HasColumnName("delivery_date").IsRequired();
        builder.Property(o => o.DeliveryTime).HasColumnName("delivery_time").IsRequired();

        builder.Property(o => o.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();
        builder.Property(o => o.Reference).HasColumnName("reference").HasMaxLength(200).IsRequired();

        builder.Property(o => o.RequiredCapacity)
            .HasColumnName("required_capacity")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                capacity => capacity.ToCode(),
                code => CapacityLevelExtensions.FromCode(code));

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                status => status.ToCode(),
                code => DeliveryStatusExtensions.FromCode(code));

        builder.Property(o => o.DriverId).HasColumnName("driver_id");

        builder.Property(o => o.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);

        builder.Property(o => o.ProofPhotoUrl).HasColumnName("proof_photo_url").HasMaxLength(1000);
        builder.Property(o => o.DigitalSignatureUrl).HasColumnName("digital_signature_url").HasMaxLength(1000);

        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.DriverId);
        builder.HasIndex(o => o.DeliveryDate);

        builder.Ignore(o => o.HasDeliveryProof);
    }
}
