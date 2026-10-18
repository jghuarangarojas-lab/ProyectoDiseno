using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsInventory.Domain.Entities;

namespace MsInventory.Infrastructure.Persistence.Configurations;

public sealed class ProductPlantConfiguration : IEntityTypeConfiguration<ProductPlant>
{
    public void Configure(EntityTypeBuilder<ProductPlant> builder)
    {
        builder.ToTable("product_plants");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.CommonName).HasColumnName("common_name").HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.CommonName);

        builder.Property(p => p.ImageUrl).HasColumnName("image_url").HasMaxLength(500);

        builder.Property(p => p.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Category);

        builder.Property(p => p.StockAvailable).HasColumnName("stock_available").HasPrecision(18, 2);
        builder.Property(p => p.StockMinimum).HasColumnName("stock_minimum").HasPrecision(18, 2);

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(p => p.IsBelowMinimum);

        builder.HasMany(p => p.Entries)
            .WithOne(e => e.Product)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.MermaRecords)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
