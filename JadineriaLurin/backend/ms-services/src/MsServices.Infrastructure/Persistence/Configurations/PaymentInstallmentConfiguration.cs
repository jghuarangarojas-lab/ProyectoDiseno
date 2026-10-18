using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Infrastructure.Persistence.Configurations;

public sealed class PaymentInstallmentConfiguration : IEntityTypeConfiguration<PaymentInstallment>
{
    public void Configure(EntityTypeBuilder<PaymentInstallment> builder)
    {
        builder.ToTable("payment_installments");

        builder.HasKey(i => i.InstallmentId);
        builder.Property(i => i.InstallmentId).HasColumnName("installment_id").ValueGeneratedNever();

        builder.Property(i => i.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(i => i.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.DueDate).HasColumnName("due_date").IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                status => status.ToCode(),
                code => code.ToInstallmentStatus());

        builder.Property(i => i.PaidAt).HasColumnName("paid_at");
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.DueDate);
    }
}
