namespace Finitech.BuildingBlocks.Domain.Security;

/// <summary>
/// Service for secure password hashing using Argon2id (OWASP recommended)
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password with a unique salt using Argon2id
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hash string containing salt and hash (format: $argon2id$v=19$m=...,t=...,p=...$salt$hash)</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash in constant time to prevent timing attacks
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Stored hash</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Checks if a password hash needs rehashing (parameters changed)
    /// </summary>
    /// <param name="hash">Stored hash</param>
    /// <returns>True if rehashing is recommended</returns>
    bool NeedsRehash(string hash);
}

/// <summary>
/// Result of password verification with additional metadata
/// </summary>
public record PasswordVerificationResult
{
    public required bool IsValid { get; init; }
    public required bool NeedsRehash { get; init; }
    public string? FailureReason { get; init; }
}
