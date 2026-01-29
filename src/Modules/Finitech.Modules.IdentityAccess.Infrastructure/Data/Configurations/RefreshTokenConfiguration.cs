using Finitech.Modules.IdentityAccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finitech.Modules.IdentityAccess.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "identity");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(256);

        builder.Property(rt => rt.ReasonRevoked)
            .HasMaxLength(256);

        // Indexes
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_TokenHash");

        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_User_Active");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
    }
}
