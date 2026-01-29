using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.Modules.Ledger.Domain;

public class AccountBalance : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public long BalanceMinorUnits { get; private set; }
    public long ReservedAmountMinorUnits { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public long Version { get; private set; } // Optimistic concurrency

    private AccountBalance() { } // EF Core

    public static AccountBalance Create(Guid accountId, string currencyCode)
    {
        return new AccountBalance
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CurrencyCode = currencyCode,
            BalanceMinorUnits = 0,
            ReservedAmountMinorUnits = 0,
            LastUpdatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    public long AvailableBalanceMinorUnits => BalanceMinorUnits - ReservedAmountMinorUnits;

    public void Credit(long amountMinorUnits)
    {
        if (amountMinorUnits <= 0)
            throw new ArgumentException("Credit amount must be positive", nameof(amountMinorUnits));

        var previousBalance = BalanceMinorUnits;
        BalanceMinorUnits += amountMinorUnits;
        LastUpdatedAt = DateTime.UtcNow;
        Version++;

        AddDomainEvent(new Events.AccountBalanceUpdatedEvent(
            AccountId,
            CurrencyCode,
            previousBalance,
            BalanceMinorUnits,
            amountMinorUnits));
    }

    public void Debit(long amountMinorUnits)
    {
        if (amountMinorUnits <= 0)
            throw new ArgumentException("Debit amount must be positive", nameof(amountMinorUnits));

        if (amountMinorUnits > BalanceMinorUnits)
            throw new InvalidOperationException("Insufficient balance");

        var previousBalance = BalanceMinorUnits;
        BalanceMinorUnits -= amountMinorUnits;
        LastUpdatedAt = DateTime.UtcNow;
        Version++;

        AddDomainEvent(new Events.AccountBalanceUpdatedEvent(
            AccountId,
            CurrencyCode,
            previousBalance,
            BalanceMinorUnits,
            -amountMinorUnits));
    }

    public void Reserve(long amountMinorUnits)
    {
        if (amountMinorUnits <= 0)
            throw new ArgumentException("Reserve amount must be positive", nameof(amountMinorUnits));

        if (amountMinorUnits > AvailableBalanceMinorUnits)
            throw new InvalidOperationException("Insufficient available balance for reservation");

        ReservedAmountMinorUnits += amountMinorUnits;
        LastUpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void ReleaseReservation(long amountMinorUnits)
    {
        if (amountMinorUnits <= 0)
            throw new ArgumentException("Release amount must be positive", nameof(amountMinorUnits));

        if (amountMinorUnits > ReservedAmountMinorUnits)
            throw new InvalidOperationException("Cannot release more than reserved");

        ReservedAmountMinorUnits -= amountMinorUnits;
        LastUpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public bool HasSufficientBalance(long amountMinorUnits) =>
        AvailableBalanceMinorUnits >= amountMinorUnits;
}
