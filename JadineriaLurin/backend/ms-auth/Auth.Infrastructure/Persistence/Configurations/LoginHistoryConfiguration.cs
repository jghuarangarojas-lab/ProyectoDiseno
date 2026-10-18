using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("login_histories");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(l => l.IpAddress).HasMaxLength(45);
        builder.Property(l => l.TimestampUtc).IsRequired();

        builder.HasIndex(l => l.UserId);

        builder.HasOne(l => l.User)
            .WithMany(u => u.LoginHistories)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
