namespace Finitech.Modules.IdentityAccess.Contracts.DTOs;

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid PartyId { get; init; }
}

public record RegisterResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record LoginRequest
{
    public string EmailOrPhone { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? DeviceId { get; init; }
    public string? IpAddress { get; init; }
}

public record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}

public record ForgotPasswordResponse
{
    public string ResetToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

public record ResetPasswordRequest
{
    public string ResetToken { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record ChangeContactRequest
{
    public string? NewEmail { get; init; }
    public string? NewPhoneNumber { get; init; }
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
