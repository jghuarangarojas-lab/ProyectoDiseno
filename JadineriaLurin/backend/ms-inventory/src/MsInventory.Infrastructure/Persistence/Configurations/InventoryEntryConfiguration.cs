using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsInventory.Domain.Entities;

namespace MsInventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryEntryConfiguration : IEntityTypeConfiguration<InventoryEntry>
{
    public void Configure(EntityTypeBuilder<InventoryEntry> builder)
    {
        builder.ToTable("inventory_entries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(18, 2).IsRequired();

        builder.Property(e => e.Supplier).HasColumnName("supplier").HasMaxLength(200).IsRequired();
        builder.HasIndex(e => e.Supplier);

        builder.Property(e => e.EntryDate).HasColumnName("entry_date").IsRequired();
        builder.HasIndex(e => e.EntryDate);

        builder.Property(e => e.ExpirationDate).HasColumnName("expiration_date");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
