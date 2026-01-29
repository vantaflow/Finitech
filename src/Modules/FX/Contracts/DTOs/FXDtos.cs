namespace Finitech.Modules.FX.Contracts.DTOs;

public record FXRateDto
{
    public string FromCurrencyCode { get; init; } = string.Empty;
    public string ToCurrencyCode { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public decimal InverseRate { get; init; }
    public DateTime EffectiveAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record FXQuoteRequest
{
    public string FromCurrencyCode { get; init; } = string.Empty;
    public string ToCurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
}

public record FXQuoteResponse
{
    public Guid QuoteId { get; init; }
    public string FromCurrencyCode { get; init; } = string.Empty;
    public string ToCurrencyCode { get; init; } = string.Empty;
    public long SourceAmountMinorUnits { get; init; }
    public decimal SourceAmountDecimal { get; init; }
    public long TargetAmountMinorUnits { get; init; }
    public decimal TargetAmountDecimal { get; init; }
    public decimal Rate { get; init; }
    public long FeeMinorUnits { get; init; }
    public decimal FeeDecimal { get; init; }
    public long NetAmountMinorUnits { get; init; }
    public decimal NetAmountDecimal { get; init; }
    public DateTime ValidUntil { get; init; }
}

public record FXConvertRequest
{
    public Guid QuoteId { get; init; }
    public Guid? SourceAccountId { get; init; }
    public Guid? TargetAccountId { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record FXConvertResponse
{
    public Guid ConversionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? SourceLedgerEntryId { get; init; }
    public Guid? TargetLedgerEntryId { get; init; }
    public Guid? FeeLedgerEntryId { get; init; }
}

public record FXRateRequest
{
    public string FromCurrencyCode { get; init; } = string.Empty;
    public string ToCurrencyCode { get; init; } = string.Empty;
    public DateTime? AtTime { get; init; }
}
