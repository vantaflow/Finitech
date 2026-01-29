namespace Finitech.Modules.WalletFMCG.Contracts.DTOs;

public record FloatAccountDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string AccountType { get; init; } = string.Empty; // RetailAgent, Distributor, Institution
    public string Status { get; init; } = string.Empty;
    public List<FloatBalanceDto> Balances { get; init; } = new();
    public decimal? MinBalanceAlertThreshold { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record FloatBalanceDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public long BalanceMinorUnits { get; init; }
    public decimal BalanceDecimal { get; init; }
}

public record CashInRequest
{
    public Guid AgentId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? CustomerWalletId { get; init; }
    public string? Reference { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record CashOutRequest
{
    public Guid AgentId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? CustomerWalletId { get; init; }
    public string? Reference { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record CashOperationResultDto
{
    public Guid OperationId { get; init; }
    public string OperationType { get; init; } = string.Empty; // CashIn, CashOut
    public string Status { get; init; } = string.Empty;
    public long AgentNewBalanceMinorUnits { get; init; }
    public long? CustomerNewBalanceMinorUnits { get; init; }
    public DateTime ExecutedAt { get; init; }
}

public record NetworkHierarchyDto
{
    public Guid DistributorId { get; init; }
    public string DistributorName { get; init; } = string.Empty;
    public int AgentCount { get; init; }
    public int MerchantCount { get; init; }
    public List<AgentInNetworkDto> Agents { get; init; } = new();
}

public record AgentInNetworkDto
{
    public Guid AgentId { get; init; }
    public string AgentName { get; init; } = string.Empty;
    public int MerchantCount { get; init; }
    public List<MerchantInNetworkDto> Merchants { get; init; } = new();
}

public record MerchantInNetworkDto
{
    public Guid MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public string BusinessType { get; init; } = string.Empty;
    public DateTime AddedAt { get; init; }
}

public record CommissionDto
{
    public Guid Id { get; init; }
    public Guid BeneficiaryId { get; init; }
    public string BeneficiaryType { get; init; } = string.Empty; // Agent, Distributor
    public string OperationType { get; init; } = string.Empty; // CashIn, CashOut, MerchantPay, BillPay, TopUp
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public decimal CommissionRate { get; init; }
    public long CommissionAmountMinorUnits { get; init; }
    public Guid? OriginalTransactionId { get; init; }
    public DateTime CalculatedAt { get; init; }
    public string Status { get; init; } = string.Empty; // Pending, Paid, Reversed
}

public record CommissionRuleDto
{
    public string OperationType { get; init; } = string.Empty;
    public string BeneficiaryType { get; init; } = string.Empty;
    public decimal CommissionRate { get; init; }
    public long? MinAmountMinorUnits { get; init; }
    public long? MaxAmountMinorUnits { get; init; }
    public bool IsActive { get; init; }
}

public record FloatAlertDto
{
    public Guid FloatAccountId { get; init; }
    public string AlertType { get; init; } = string.Empty; // LowBalance, ZeroBalance, AbnormalActivity
    public string Message { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
}
