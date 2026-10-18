using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Infrastructure.Persistence.Configurations;

public sealed class ProjectAttachmentConfiguration : IEntityTypeConfiguration<ProjectAttachment>
{
    public void Configure(EntityTypeBuilder<ProjectAttachment> builder)
    {
        builder.ToTable("project_attachments");

        builder.HasKey(a => a.AttachmentId);
        builder.Property(a => a.AttachmentId).HasColumnName("attachment_id").ValueGeneratedNever();

        builder.Property(a => a.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(a => a.FileUrl).HasColumnName("file_url").HasMaxLength(1000).IsRequired();

        builder.Property(a => a.FileType)
            .HasColumnName("file_type")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                fileType => fileType.ToCode(),
                code => code.ToFileType());

        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(300).IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("content_type").HasMaxLength(120);
        builder.Property(a => a.SizeBytes).HasColumnName("size_bytes");
        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at");

        builder.HasIndex(a => a.ProjectId);
        builder.HasIndex(a => a.FileType);
    }
}
