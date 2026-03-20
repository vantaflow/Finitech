namespace Finitech.Modules.Ledger.Domain.Events;

/// <summary>
/// Immutable event in the ledger event store.
/// Uses PostgreSQL jsonb for flexible event data storage.
/// </summary>
public class LedgerEvent
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = "LedgerAccount";
    public string EventType { get; set; } = string.Empty;
    public long Version { get; set; }
    public string Data { get; set; } = "{}";
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
}

public static class LedgerEventTypes
{
    public const string AccountOpened = "AccountOpened";
    public const string MoneyDeposited = "MoneyDeposited";
    public const string MoneyWithdrawn = "MoneyWithdrawn";
    public const string TransferExecuted = "TransferExecuted";
    public const string InterestAccrued = "InterestAccrued";
    public const string AccountFrozen = "AccountFrozen";
    public const string AccountClosed = "AccountClosed";
    public const string FxConversionExecuted = "FxConversionExecuted";
    public const string RefundProcessed = "RefundProcessed";
    public const string ChargebackInitiated = "ChargebackInitiated";
}
