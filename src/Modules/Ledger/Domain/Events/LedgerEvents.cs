using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.Modules.Ledger.Domain.Events;

/// <summary>
/// Event raised when a new ledger entry is created.
/// </summary>
public record LedgerEntryCreatedEvent : DomainEvent
{
    public Guid EntryId { get; }
    public Guid AccountId { get; }
    public string CurrencyCode { get; }
    public long AmountMinorUnits { get; }
    public string EntryType { get; }
    public long RunningBalance { get; }
    public string? Reference { get; }

    public LedgerEntryCreatedEvent(
        Guid entryId,
        Guid accountId,
        string currencyCode,
        long amountMinorUnits,
        string entryType,
        long runningBalance,
        string? reference = null)
    {
        EntryId = entryId;
        AccountId = accountId;
        CurrencyCode = currencyCode;
        AmountMinorUnits = amountMinorUnits;
        EntryType = entryType;
        RunningBalance = runningBalance;
        Reference = reference;
    }
}

/// <summary>
/// Event raised when an account balance is updated.
/// </summary>
public record AccountBalanceUpdatedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public string CurrencyCode { get; }
    public long PreviousBalance { get; }
    public long NewBalance { get; }
    public long ChangeAmount { get; }

    public AccountBalanceUpdatedEvent(
        Guid accountId,
        string currencyCode,
        long previousBalance,
        long newBalance,
        long changeAmount)
    {
        AccountId = accountId;
        CurrencyCode = currencyCode;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        ChangeAmount = changeAmount;
    }
}

/// <summary>
/// Event raised when a transaction is voided/reversed.
/// </summary>
public record LedgerEntryVoidedEvent : DomainEvent
{
    public Guid OriginalEntryId { get; }
    public Guid VoidEntryId { get; }
    public Guid AccountId { get; }
    public string Reason { get; }

    public LedgerEntryVoidedEvent(
        Guid originalEntryId,
        Guid voidEntryId,
        Guid accountId,
        string reason)
    {
        OriginalEntryId = originalEntryId;
        VoidEntryId = voidEntryId;
        AccountId = accountId;
        Reason = reason;
    }
}
