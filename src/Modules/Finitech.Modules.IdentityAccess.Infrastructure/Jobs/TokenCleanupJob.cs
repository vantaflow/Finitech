using Finitech.Modules.IdentityAccess.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Finitech.Modules.IdentityAccess.Infrastructure.Jobs;

/// <summary>
/// Daily job to cleanup expired refresh tokens and sessions
/// </summary>
[DisallowConcurrentExecution]
public class TokenCleanupJob : IJob
{
    private readonly IdentityDbContext _dbContext;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(IdentityDbContext dbContext, ILogger<TokenCleanupJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting token cleanup job at {Time}", DateTime.UtcNow);

        // Delete expired refresh tokens older than 7 days
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var expiredTokens = await _dbContext.RefreshTokens
            .Where(t => t.ExpiresAt < cutoffDate || (t.RevokedAt != null && t.RevokedAt < cutoffDate))
            .ToListAsync(context.CancellationToken);

        _dbContext.RefreshTokens.RemoveRange(expiredTokens);
        int tokenCount = expiredTokens.Count;

        // Delete expired sessions
        var expiredSessions = await _dbContext.UserSessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow || s.TerminatedAt != null)
            .Where(s => s.LastActivityAt == null || s.LastActivityAt < DateTime.UtcNow.AddDays(-30))
            .ToListAsync(context.CancellationToken);

        _dbContext.UserSessions.RemoveRange(expiredSessions);
        int sessionCount = expiredSessions.Count;

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Token cleanup completed. Removed {TokenCount} tokens and {SessionCount} sessions",
            tokenCount, sessionCount);
    }
}
