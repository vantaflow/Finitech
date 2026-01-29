using Finitech.BuildingBlocks.SharedKernel.Primitives;
using Finitech.Modules.Ledger.Domain.Events;

namespace Finitech.Modules.Ledger.Domain;

public class LedgerEntry : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public LedgerEntryType EntryType { get; private set; }
    public long AmountMinorUnits { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Reference { get; private set; }
    public Guid? TransactionId { get; private set; }
    public Guid? OriginalEntryId { get; private set; } // For voids/reversals
    public DateTime EntryDate { get; private set; }
    public long RunningBalance { get; private set; }
    public string Status { get; private set; } = LedgerEntryStatus.Posted;
    public string? IdempotencyKey { get; private set; }

    private LedgerEntry() { } // EF Core

    public static LedgerEntry Create(
        Guid accountId,
        string currencyCode,
        LedgerEntryType entryType,
        long amountMinorUnits,
        string description,
        string? reference,
        long previousBalance,
        string? idempotencyKey = null)
    {
        if (amountMinorUnits <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amountMinorUnits));

        var runningBalance = entryType == LedgerEntryType.Credit
            ? previousBalance + amountMinorUnits
            : previousBalance - amountMinorUnits;

        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CurrencyCode = currencyCode,
            EntryType = entryType,
            AmountMinorUnits = amountMinorUnits,
            Description = description,
            Reference = reference,
            EntryDate = DateTime.UtcNow,
            RunningBalance = runningBalance,
            IdempotencyKey = idempotencyKey
        };

        // Raise domain event for outbox pattern
        entry.AddDomainEvent(new LedgerEntryCreatedEvent(
            entry.Id,
            entry.AccountId,
            entry.CurrencyCode,
            entry.AmountMinorUnits,
            entry.EntryType.ToString(),
            entry.RunningBalance,
            entry.Reference));

        return entry;
    }

    public static LedgerEntry CreateVoid(
        LedgerEntry originalEntry,
        string reason,
        long previousBalance)
    {
        var voidEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            AccountId = originalEntry.AccountId,
            CurrencyCode = originalEntry.CurrencyCode,
            EntryType = originalEntry.EntryType == LedgerEntryType.Credit
                ? LedgerEntryType.Debit
                : LedgerEntryType.Credit,
            AmountMinorUnits = originalEntry.AmountMinorUnits,
            Description = $"VOID: {reason}",
            Reference = $"VOID-{originalEntry.Id}",
            OriginalEntryId = originalEntry.Id,
            EntryDate = DateTime.UtcNow,
            RunningBalance = previousBalance + (originalEntry.EntryType == LedgerEntryType.Credit
                ? -originalEntry.AmountMinorUnits
                : originalEntry.AmountMinorUnits)
        };

        // Raise domain event
        voidEntry.AddDomainEvent(new LedgerEntryVoidedEvent(
            originalEntry.Id,
            voidEntry.Id,
            voidEntry.AccountId,
            reason));

        return voidEntry;
    }
}

public enum LedgerEntryType
{
    Debit,
    Credit
}

public static class LedgerEntryStatus
{
    public const string Posted = "Posted";
    public const string Pending = "Pending";
    public const string Reversed = "Reversed";
}
