using Finitech.Modules.IdentityAccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finitech.Modules.IdentityAccess.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "identity");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.EncryptedPhoneNumber)
            .HasMaxLength(256);

        builder.Property(u => u.PhoneNumberHash)
            .HasMaxLength(64);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(64);

        builder.Property(u => u.SecurityStamp)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(u => u.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes
        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("IX_Users_NormalizedEmail");

        builder.HasIndex(u => u.PhoneNumberHash)
            .HasDatabaseName("IX_Users_PhoneNumberHash");

        builder.HasIndex(u => new { u.Status, u.LastLoginAt })
            .HasDatabaseName("IX_Users_Status_LastLogin");

        // Relationships
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
