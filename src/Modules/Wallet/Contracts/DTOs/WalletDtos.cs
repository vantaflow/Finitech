namespace Finitech.Modules.Wallet.Contracts.DTOs;

public record WalletAccountDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string WalletLevel { get; init; } = string.Empty; // Basic, Standard, Premium
    public string Status { get; init; } = string.Empty; // Active, Suspended, Closed
    public List<WalletBalanceDto> Balances { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record WalletBalanceDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public long BalanceMinorUnits { get; init; }
    public decimal BalanceDecimal { get; init; }
}

public record WalletLimitsDto
{
    public string LimitType { get; init; } = string.Empty; // CashIn, CashOut, P2PSend, P2PReceive, MerchantPay, BillPay
    public string CurrencyCode { get; init; } = string.Empty;
    public long DailyLimitMinorUnits { get; init; }
    public long MonthlyLimitMinorUnits { get; init; }
    public long DailyUsedMinorUnits { get; init; }
    public long MonthlyUsedMinorUnits { get; init; }
}

public record CreateWalletRequest
{
    public Guid PartyId { get; init; }
    public string InitialLevel { get; init; } = "Basic";
    public List<string> SupportedCurrencies { get; init; } = new() { "MAD" };
}

public record P2PSendRequest
{
    public Guid FromWalletId { get; init; }
    public string ToIdentifier { get; init; } = string.Empty; // Phone, Email, or WalletId
    public string IdentifierType { get; init; } = string.Empty; // Phone, Email, WalletId
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? Description { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record P2PRequestMoneyRequest
{
    public Guid FromWalletId { get; init; }
    public string ToIdentifier { get; init; } = string.Empty;
    public string IdentifierType { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? Description { get; init; }
}

public record P2PRequestDto
{
    public Guid Id { get; init; }
    public Guid RequesterWalletId { get; init; }
    public Guid TargetWalletId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string Status { get; init; } = string.Empty; // Pending, Accepted, Rejected, Expired
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record RespondToP2PRequest
{
    public Guid RequestId { get; init; }
    public string Response { get; init; } = string.Empty; // Accept, Reject
}

public record SplitPaymentRequest
{
    public Guid InitiatorWalletId { get; init; }
    public List<string> ParticipantIdentifiers { get; init; } = new();
    public string CurrencyCode { get; init; } = string.Empty;
    public long TotalAmountMinorUnits { get; init; }
    public string? Description { get; init; }
}

public record SplitPaymentDto
{
    public Guid Id { get; init; }
    public Guid InitiatorWalletId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long TotalAmountMinorUnits { get; init; }
    public string Status { get; init; } = string.Empty;
    public List<SplitParticipantDto> Participants { get; init; } = new();
}

public record SplitParticipantDto
{
    public Guid WalletId { get; init; }
    public long AmountMinorUnits { get; init; }
    public bool HasPaid { get; init; }
}

public record ScheduledWalletPaymentDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public string PaymentType { get; init; } = string.Empty; // Bill, TopUp, P2P
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string Frequency { get; init; } = string.Empty; // Once, Daily, Weekly, Monthly
    public DateTime NextExecutionAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record CreateScheduledPaymentRequest
{
    public Guid WalletId { get; init; }
    public string PaymentType { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string Frequency { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public string? RecipientIdentifier { get; init; }
}

public record LoyaltyPointsDto
{
    public Guid WalletId { get; init; }
    public long AvailablePoints { get; init; }
    public long LifetimePoints { get; init; }
    public string Tier { get; init; } = string.Empty; // Bronze, Silver, Gold, Platinum
    public decimal TierProgress { get; init; }
}

public record LoyaltyTransactionDto
{
    public Guid Id { get; init; }
    public long Points { get; init; }
    public string TransactionType { get; init; } = string.Empty; // Earn, Redeem, Expire
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record RedeemPointsRequest
{
    public Guid WalletId { get; init; }
    public long Points { get; init; }
    public string RedemptionType { get; init; } = string.Empty; // Cashback, Voucher
}

public record NFCTokenDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
