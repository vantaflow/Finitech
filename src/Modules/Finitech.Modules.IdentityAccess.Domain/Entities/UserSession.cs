namespace Finitech.Modules.IdentityAccess.Domain.Entities;

/// <summary>
/// User session tracking for security
/// </summary>
public class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceType { get; set; }
    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public string? TerminatedBy { get; set; }

    public bool IsActive => TerminatedAt == null && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);

    // Navigation property
    public User User { get; set; } = null!;
}
