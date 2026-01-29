namespace Finitech.Modules.Ledger.Contracts.DTOs;

public record BalanceDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public decimal AmountDecimal { get; init; }
    public int CurrencyNumericCode { get; init; }
}

public record GetBalancesResponse
{
    public Guid AccountId { get; init; }
    public List<BalanceDto> Balances { get; init; } = new();
}

public record GetHistoryRequest
{
    public string? CurrencyCode { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? TransactionType { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 50;
}

public record LedgerEntryDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string EntryType { get; init; } = string.Empty; // Debit, Credit
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public decimal AmountDecimal { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public Guid? TransactionId { get; init; }
    public Guid? OriginalTransactionId { get; init; } // For voids/reversals
    public DateTime EntryDate { get; init; }
    public long RunningBalance { get; init; }
}

public record GetHistoryResponse
{
    public Guid AccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public List<LedgerEntryDto> Entries { get; init; } = new();
    public int TotalCount { get; init; }
}

public record PostTransactionRequest
{
    public Guid AccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string EntryType { get; init; } = string.Empty; // Debit, Credit
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record PostTransactionResponse
{
    public Guid TransactionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public long NewBalanceMinorUnits { get; init; }
}

public record VoidTransactionRequest
{
    public Guid OriginalTransactionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record AdjustmentRequest
{
    public Guid AccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string AdjustmentType { get; init; } = string.Empty; // Correction, Fee, Interest
    public string Reason { get; init; } = string.Empty;
    public string? ApprovedBy { get; init; }
}
