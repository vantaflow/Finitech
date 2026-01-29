namespace Finitech.Modules.MerchantPayments.Contracts.DTOs;

public record MerchantDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string BusinessName { get; init; } = string.Empty;
    public string MerchantCode { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // Active, Suspended, Closed
    public DateTime CreatedAt { get; init; }
}

public record CreateMerchantRequest
{
    public Guid PartyId { get; init; }
    public string BusinessName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? MerchantCode { get; init; }
}

public record GenerateDynamicQRRequest
{
    public Guid MerchantId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty; // MAD, EUR, USD
    public long AmountMinorUnits { get; init; }
    public string? Reference { get; init; }
    public string? Description { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record DynamicQRDto
{
    public string Payload { get; init; } = string.Empty;
    public string PayloadFormat { get; init; } = string.Empty; // EMVCo
    public int PayloadLength { get; init; }
    public string CurrencyNumericCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string CRC { get; init; } = string.Empty;
}

public record ParsedQRDto
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string PayloadFormatIndicator { get; init; } = string.Empty;
    public string PointOfInitiationMethod { get; init; } = string.Empty; // 11=static, 12=dynamic
    public string? MerchantAccountInformation { get; init; }
    public string? MerchantCategoryCode { get; init; }
    public string TransactionCurrency { get; init; } = string.Empty;
    public decimal? TransactionAmount { get; init; }
    public string? CountryCode { get; init; }
    public string? MerchantName { get; init; }
    public string? MerchantCity { get; init; }
    public string? AdditionalData { get; init; }
    public string? TransactionReference { get; init; }
    public string CRC { get; init; } = string.Empty;
    public bool CrcValid { get; init; }
}

public record PayByQRRequest
{
    public string QRPayload { get; init; } = string.Empty;
    public Guid PayerWalletId { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record PayByQRResponse
{
    public Guid TransactionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? MerchantName { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public DateTime ExecutedAt { get; init; }
}
