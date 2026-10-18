using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsInventory.Domain.Entities;
using MsInventory.Domain.Enums;

namespace MsInventory.Infrastructure.Persistence.Configurations;

public sealed class MermaRecordConfiguration : IEntityTypeConfiguration<MermaRecord>
{
    public void Configure(EntityTypeBuilder<MermaRecord> builder)
    {
        builder.ToTable("merma_records");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.ProductId).HasColumnName("product_id").IsRequired();

        builder.Property(m => m.QuantityLost).HasColumnName("quantity_lost").HasPrecision(18, 2).IsRequired();

        builder.Property(m => m.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();

        builder.Property(m => m.EvidencePhotoUrl).HasColumnName("evidence_photo_url").HasMaxLength(1000);

        builder.Property(m => m.ManagerReviewStatus)
            .HasColumnName("manager_review_status")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                status => status.ToCode(),
                code => ManagerReviewStatusExtensions.FromCode(code));

        builder.HasIndex(m => m.ManagerReviewStatus);

        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(m => m.CreatedAt);
    }
}
