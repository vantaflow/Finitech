using Finitech.BuildingBlocks.SharedKernel.Primitives;
using System.Security.Cryptography;

namespace Finitech.Modules.IdentityAccess.Domain;

public class User : AggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid PartyId { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    private User() { } // EF Core

    public static User Create(string email, string? phoneNumber, string password, Guid partyId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PhoneNumber = phoneNumber,
            PasswordHash = HashPassword(password),
            PartyId = partyId,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            FailedLoginAttempts = 0
        };
    }

    public bool ValidatePassword(string password)
    {
        // Simple hash comparison (use proper library in production like BCrypt or Argon2)
        var hash = HashPassword(password);
        return PasswordHash == hash;
    }

    public void RecordLoginSuccess()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void RecordLoginFailure(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            AddDomainEvent(new AccountLockedEvent(Id, Email, "Too many failed login attempts", LockedUntil.Value));
        }
    }

    public bool IsLocked() =>
        Status == UserStatus.Locked ||
        (LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow);

    public void Lock(string reason, string? adminId = null)
    {
        Status = UserStatus.Locked;
        LockedUntil = null; // Permanent lock
        AddDomainEvent(new AccountLockedEvent(Id, Email, reason, null, adminId));
    }

    public void Unlock(string? adminId = null)
    {
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        AddDomainEvent(new AccountUnlockedEvent(Id, Email, adminId));
    }

    public void ChangePassword(string newPassword)
    {
        PasswordHash = HashPassword(newPassword);
    }

    public void UpdateContact(string? newEmail, string? newPhoneNumber)
    {
        if (!string.IsNullOrEmpty(newEmail))
            Email = newEmail.ToLowerInvariant();
        if (newPhoneNumber != null)
            PhoneNumber = newPhoneNumber;
    }

    public void GenerateRefreshToken(TimeSpan duration)
    {
        RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        RefreshTokenExpiresAt = DateTime.UtcNow.Add(duration);
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public bool ValidateRefreshToken(string token) =>
        RefreshToken == token &&
        RefreshTokenExpiresAt.HasValue &&
        RefreshTokenExpiresAt.Value > DateTime.UtcNow;

    private static string HashPassword(string password)
    {
        // Simplified - use proper password hashing in production (BCrypt, Argon2, PBKDF2)
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "fixed-salt-change-in-production");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}

public enum UserStatus
{
    Active,
    Locked,
    Suspended,
    Closed
}

public record AccountLockedEvent(Guid UserId, string Email, string Reason, DateTime? LockedUntil, string? AdminId = null) : DomainEvent;
public record AccountUnlockedEvent(Guid UserId, string Email, string? AdminId = null) : DomainEvent;
