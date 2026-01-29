using Finitech.Modules.IdentityAccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finitech.Modules.IdentityAccess.Infrastructure.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions", "identity");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);

        builder.Property(s => s.DeviceId)
            .HasMaxLength(100);

        builder.Property(s => s.DeviceType)
            .HasMaxLength(50);

        builder.Property(s => s.Location)
            .HasMaxLength(100);

        builder.Property(s => s.TerminatedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(s => s.SessionId)
            .IsUnique()
            .HasDatabaseName("IX_UserSessions_SessionId");

        builder.HasIndex(s => new { s.UserId, s.IsActive })
            .HasDatabaseName("IX_UserSessions_User_Active");

        builder.HasIndex(s => s.ExpiresAt)
            .HasDatabaseName("IX_UserSessions_ExpiresAt");
    }
}
