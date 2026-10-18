using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsServices.Domain.Entities;

namespace MsServices.Infrastructure.Persistence.Configurations;

public sealed class ContractDocumentConfiguration : IEntityTypeConfiguration<ContractDocument>
{
    public void Configure(EntityTypeBuilder<ContractDocument> builder)
    {
        builder.ToTable("contract_documents");

        builder.HasKey(c => c.ContractId);
        builder.Property(c => c.ContractId).HasColumnName("contract_id").ValueGeneratedNever();

        builder.Property(c => c.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(c => c.GeneratedPdfUrl).HasColumnName("generated_pdf_url").HasMaxLength(1000);
        builder.Property(c => c.DigitalSignatureUrl).HasColumnName("digital_signature_url").HasMaxLength(1000);
        builder.Property(c => c.SignedAt).HasColumnName("signed_at");
        builder.Property(c => c.SignedBy).HasColumnName("signed_by").HasMaxLength(200);
        builder.Property(c => c.GeneratedAt).HasColumnName("generated_at");

        builder.HasIndex(c => c.ProjectId).IsUnique();
    }
}
