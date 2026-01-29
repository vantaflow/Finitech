namespace Finitech.Modules.IdentityAccess.Domain.Entities;

/// <summary>
/// User entity for authentication and identity management
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? EncryptedPhoneNumber { get; set; }
    public string? PhoneNumberHash { get; set; } // For lookups

    // Status tracking
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? LockedBy { get; set; }
    public string? LockReason { get; set; }

    // Verification flags
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }

    // Security stamp for concurrent session invalidation
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    // Concurrency token
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public enum UserStatus
{
    Active,
    Inactive,
    Locked,
    Suspended,
    PendingVerification
}
