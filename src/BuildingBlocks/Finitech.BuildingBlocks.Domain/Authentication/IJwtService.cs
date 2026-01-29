namespace Finitech.BuildingBlocks.Domain.Authentication;

public interface IJwtService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    /// <param name="permissions">User permissions</param>
    /// <returns>Access token with metadata</returns>
    TokenResult GenerateAccessToken(Guid userId, string email, string[] roles, string[] permissions);

    /// <summary>
    /// Generates a refresh token for token rotation
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Refresh token with expiration</returns>
    RefreshTokenResult GenerateRefreshToken(Guid userId);

    /// <summary>
    /// Validates an access token and returns the principal
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Token principal if valid, null otherwise</returns>
    TokenPrincipal? ValidateAccessToken(string token);

    /// <summary>
    /// Validates a refresh token
    /// </summary>
    /// <param name="token">Refresh token</param>
    /// <returns>User ID if valid, null otherwise</returns>
    Guid? ValidateRefreshToken(string token);

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    /// <param name="token">Refresh token to revoke</param>
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record TokenResult
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string TokenType { get; init; } = "Bearer";
}

public record RefreshTokenResult
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string TokenId { get; init; }
}

public record TokenPrincipal
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string[] Roles { get; init; }
    public required string[] Permissions { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
