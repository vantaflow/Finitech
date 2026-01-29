using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.Modules.Banking.Domain.Entities;

/// <summary>
/// Bank account aggregate root
/// </summary>
public class BankAccount : AggregateRoot
{
    public new Guid Id { get; private set; }
    public Guid PartyId { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountType { get; private set; } = string.Empty; // Current, Savings, FixedDeposit
    public string CurrencyCode { get; private set; } = "MAD";
    public long BalanceMinorUnits { get; private set; }
    public BankAccountStatus Status { get; private set; } = BankAccountStatus.Active;
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosedReason { get; private set; }
    public decimal InterestRate { get; private set; }
    public long OverdraftLimitMinorUnits { get; private set; }

    // Navigation
    public ICollection<Card> Cards { get; private set; } = new List<Card>();
    public ICollection<Loan> Loans { get; private set; } = new List<Loan>();

    private BankAccount() { } // EF Core

    public static BankAccount Create(Guid partyId, string accountNumber, string accountType, string currencyCode)
    {
        return new BankAccount
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            AccountNumber = accountNumber,
            AccountType = accountType,
            CurrencyCode = currencyCode,
            BalanceMinorUnits = 0,
            Status = BankAccountStatus.Active,
            OpenedAt = DateTime.UtcNow
        };
    }

    public void Deposit(long amountMinorUnits)
    {
        if (amountMinorUnits <= 0) throw new ArgumentException("Amount must be positive");
        if (Status != BankAccountStatus.Active) throw new InvalidOperationException("Account is not active");

        BalanceMinorUnits += amountMinorUnits;
    }

    public void Withdraw(long amountMinorUnits)
    {
        if (amountMinorUnits <= 0) throw new ArgumentException("Amount must be positive");
        if (Status != BankAccountStatus.Active) throw new InvalidOperationException("Account is not active");

        var availableBalance = BalanceMinorUnits + OverdraftLimitMinorUnits;
        if (availableBalance < amountMinorUnits)
            throw new InvalidOperationException("Insufficient funds");

        BalanceMinorUnits -= amountMinorUnits;
    }

    public void Close(string reason)
    {
        if (Status == BankAccountStatus.Closed) throw new InvalidOperationException("Account already closed");
        if (BalanceMinorUnits != 0) throw new InvalidOperationException("Account must have zero balance");

        Status = BankAccountStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedReason = reason;
    }
}

public enum BankAccountStatus
{
    Active,
    Inactive,
    Frozen,
    Closed
}
