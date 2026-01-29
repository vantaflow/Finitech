namespace Finitech.Modules.Wallet.Infrastructure.Entities;

public class WalletAccount
{
    public Guid Id { get; set; }
    public Guid PartyId { get; set; }
    public string WalletLevel { get; set; } = "Basic";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }

    public ICollection<WalletBalance> Balances { get; set; } = new List<WalletBalance>();
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

public class WalletBalance
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string CurrencyCode { get; set; } = "MAD";
    public long BalanceMinorUnits { get; set; }
    public long ReservedAmountMinorUnits { get; set; }
    public long AvailableBalanceMinorUnits => BalanceMinorUnits - ReservedAmountMinorUnits;

    public WalletAccount Wallet { get; set; } = null!;
}

public class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // CashIn, CashOut, P2P
    public string CurrencyCode { get; set; } = "MAD";
    public long AmountMinorUnits { get; set; }
    public long BalanceAfterMinorUnits { get; set; }
    public string? Reference { get; set; }
    public string? ExternalReference { get; set; }
    public string Status { get; set; } = "Completed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WalletAccount Wallet { get; set; } = null!;
}
