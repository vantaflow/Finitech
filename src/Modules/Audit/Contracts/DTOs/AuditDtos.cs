namespace Finitech.Modules.Audit.Contracts.DTOs;

public record AuditEntryDto
{
    public Guid Id { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string ActorType { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string? ActorIpAddress { get; init; }
    public DateTime Timestamp { get; init; }
    public string? BeforeState { get; init; }
    public string? AfterState { get; init; }
    public string? Metadata { get; init; }
}

public record AuditQueryRequest
{
    public string? ActorId { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Action { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 50;
}

public record AuditQueryResponse
{
    public List<AuditEntryDto> Entries { get; init; } = new();
    public int TotalCount { get; init; }
}

public record LogAuditRequest
{
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string ActorType { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string? ActorIpAddress { get; init; }
    public object? BeforeState { get; init; }
    public object? AfterState { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}
