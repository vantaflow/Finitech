namespace Finitech.BuildingBlocks.Domain.Idempotency;

/// <summary>
/// Idempotency key for ensuring operations are executed only once.
/// </summary>
public readonly record struct IdempotencyKey
{
    public string Value { get; }
    public string ResourceType { get; }
    public DateTime CreatedAt { get; }

    public IdempotencyKey(string value, string resourceType)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Idempotency key cannot be empty", nameof(value));
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("Resource type cannot be empty", nameof(resourceType));

        Value = value;
        ResourceType = resourceType;
        CreatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"{ResourceType}:{Value}";
}

/// <summary>
/// Record of an idempotent operation.
/// </summary>
public class IdempotentOperation
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
}

public interface IIdempotencyService
{
    Task<IdempotencyCheckResult> CheckAsync(IdempotencyKey key, CancellationToken cancellationToken = default);
    Task RecordStartAsync(IdempotencyKey key, object request, CancellationToken cancellationToken = default);
    Task RecordCompletionAsync(IdempotencyKey key, object? response, CancellationToken cancellationToken = default);
    Task RecordFailureAsync(IdempotencyKey key, string error, CancellationToken cancellationToken = default);
}

public record IdempotencyCheckResult
{
    public bool IsNew { get; init; }
    public bool IsCompleted { get; init; }
    public object? ExistingResponse { get; init; }
    public string? Error { get; init; }
}
