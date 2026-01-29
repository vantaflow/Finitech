namespace Finitech.BuildingBlocks.Domain.Audit;

/// <summary>
/// Audit entry for sensitive operations.
/// </summary>
public class AuditEntry
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActorType { get; set; } = string.Empty; // User, System, Admin
    public string ActorId { get; set; } = string.Empty;
    public string? ActorIpAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public string? BeforeState { get; set; }
    public string? AfterState { get; set; }
    public string? Metadata { get; set; }
}

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query, CancellationToken cancellationToken = default);
}

public class AuditQuery
{
    public string? ActorId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Action { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 50;
}
