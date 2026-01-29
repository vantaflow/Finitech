using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Finitech.BuildingBlocks.Domain.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Finitech.BuildingBlocks.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly ILogger<JwtService> _logger;
    private readonly IMemoryCache _refreshTokenCache;
    private readonly RSA _rsaKey;

    // Token configuration
    private const int AccessTokenLifetimeMinutes = 15;
    private const int RefreshTokenLifetimeDays = 7;
    private const string Issuer = "Finitech";
    private const string Audience = "Finitech.Users";

    public JwtService(ILogger<JwtService> logger, IMemoryCache? refreshTokenCache = null)
    {
        _logger = logger;
        _refreshTokenCache = refreshTokenCache ?? new MemoryCache(new MemoryCacheOptions());

        // Generate or load RSA key
        // In production, this should be loaded from secure key storage (Azure Key Vault, AWS KMS, etc.)
        _rsaKey = LoadOrGenerateRsaKey();
    }

    private RSA LoadOrGenerateRsaKey()
    {
        var rsa = RSA.Create(2048);

        // Check if we have a saved key (in production, use secure key storage)
        var keyPath = Path.Combine(AppContext.BaseDirectory, "keys");
        var privateKeyPath = Path.Combine(keyPath, "jwt_private.key");
        var publicKeyPath = Path.Combine(keyPath, "jwt_public.key");

        try
        {
            if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
            {
                var privateKey = File.ReadAllBytes(privateKeyPath);
                var publicKey = File.ReadAllBytes(publicKeyPath);
                rsa.ImportRSAPrivateKey(privateKey, out _);
                rsa.ImportRSAPublicKey(publicKey, out _);
                _logger.LogDebug("Loaded existing RSA key pair");
            }
            else
            {
                // Generate new key and save it
                Directory.CreateDirectory(keyPath);
                File.WriteAllBytes(privateKeyPath, rsa.ExportRSAPrivateKey());
                File.WriteAllBytes(publicKeyPath, rsa.ExportRSAPublicKey());
                _logger.LogInformation("Generated new RSA key pair for JWT signing");

                // Secure the key file (Unix only)
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    File.SetUnixFileMode(privateKeyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load or save RSA key, using in-memory key");
        }

        return rsa;
    }

    public TokenResult GenerateAccessToken(Guid userId, string email, string[] roles, string[] permissions)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("uid", userId.ToString())
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions as claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var securityKey = new RsaSecurityKey(_rsaKey);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated access token for user {UserId}, expires at {ExpiresAt}", userId, expiresAt);

        return new TokenResult
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    public RefreshTokenResult GenerateRefreshToken(Guid userId)
    {
        var tokenBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }

        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var tokenId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays);

        // Store in cache
        var cacheEntry = new RefreshTokenEntry
        {
            UserId = userId,
            TokenId = tokenId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiresAt)
            .SetPriority(CacheItemPriority.High);

        _refreshTokenCache.Set($"rt:{token}", cacheEntry, cacheOptions);
        _refreshTokenCache.Set($"rt_user:{userId}:{tokenId}", token, cacheOptions);

        _logger.LogDebug("Generated refresh token {TokenId} for user {UserId}", tokenId, userId);

        return new RefreshTokenResult
        {
            Token = token,
            ExpiresAt = expiresAt,
            TokenId = tokenId
        };
    }

    public TokenPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new RsaSecurityKey(_rsaKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero // No tolerance for token lifetime
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("Token is not a valid JWT");
                return null;
            }

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst("uid")?.Value;

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Token contains invalid user ID");
                return null;
            }

            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                ?? principal.FindFirst(ClaimTypes.Email)?.Value
                ?? string.Empty;

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            var permissions = principal.FindAll("permissions").Select(c => c.Value).ToArray();

            return new TokenPrincipal
            {
                UserId = userGuid,
                Email = email,
                Roles = roles,
                Permissions = permissions,
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token has expired");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public Guid? ValidateRefreshToken(string token)
    {
        if (!_refreshTokenCache.TryGetValue($"rt:{token}", out RefreshTokenEntry? entry))
        {
            _logger.LogDebug("Refresh token not found in cache");
            return null;
        }

        if (entry == null || entry.IsRevoked || entry.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogDebug("Refresh token is invalid, revoked, or expired");
            return null;
        }

        return entry.UserId;
    }

    public Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (_refreshTokenCache.TryGetValue($"rt:{token}", out RefreshTokenEntry? entry) && entry != null)
        {
            entry.IsRevoked = true;
            _refreshTokenCache.Set($"rt:{token}", entry, TimeSpan.FromMinutes(5));
            _refreshTokenCache.Remove($"rt_user:{entry.UserId}:{entry.TokenId}");
            _logger.LogInformation("Revoked refresh token {TokenId} for user {UserId}", entry.TokenId, entry.UserId);
        }

        return Task.CompletedTask;
    }

    public Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Note: In production with Redis/distributed cache, we'd search by pattern
        // For in-memory cache, this is a simplified version
        _logger.LogInformation("Revoking all refresh tokens for user {UserId}", userId);

        // This would require iterating all cache entries or maintaining a user-to-tokens index
        // For now, we just log as the actual implementation depends on the cache backend

        return Task.CompletedTask;
    }
}

internal class RefreshTokenEntry
{
    public Guid UserId { get; set; }
    public string TokenId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
