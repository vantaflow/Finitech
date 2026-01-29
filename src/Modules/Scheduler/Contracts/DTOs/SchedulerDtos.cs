namespace Finitech.Modules.Scheduler.Contracts.DTOs;

public record ScheduledJobDto
{
    public string JobId { get; init; } = string.Empty;
    public string JobName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CronExpression { get; init; } = string.Empty;
    public DateTime? NextExecutionAt { get; init; }
    public DateTime? LastExecutionAt { get; init; }
    public string? LastExecutionResult { get; init; }
    public bool IsActive { get; init; }
}

public record JobExecutionHistoryDto
{
    public Guid Id { get; init; }
    public string JobId { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string Status { get; init; } = string.Empty; // Running, Completed, Failed
    public string? Error { get; init; }
}

public record TriggerJobRequest
{
    public string JobId { get; init; } = string.Empty;
    public Dictionary<string, string>? Parameters { get; init; }
}
