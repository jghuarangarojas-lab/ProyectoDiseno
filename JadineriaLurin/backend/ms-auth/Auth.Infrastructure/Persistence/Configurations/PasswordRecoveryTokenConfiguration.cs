using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public class PasswordRecoveryTokenConfiguration : IEntityTypeConfiguration<PasswordRecoveryToken>
{
    public void Configure(EntityTypeBuilder<PasswordRecoveryToken> builder)
    {
        builder.ToTable("password_recovery_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token).IsRequired().HasMaxLength(128);
        builder.Property(t => t.IsUsed).IsRequired().HasDefaultValue(false);

        builder.HasIndex(t => t.Token).IsUnique();

        builder.HasOne(t => t.User)
            .WithMany(u => u.PasswordRecoveryTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
