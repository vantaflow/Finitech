namespace Finitech.Modules.Payments.Contracts.DTOs;

public record TransferRequest
{
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? Description { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record TransferResponse
{
    public Guid TransactionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime ExecutedAt { get; init; }
}

public record CrossCurrencyTransferRequest
{
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public string FromCurrencyCode { get; init; } = string.Empty;
    public string ToCurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? Description { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record BillPayRequest
{
    public Guid FromAccountId { get; init; }
    public string BillType { get; init; } = string.Empty; // Electricity, Water, Internet, etc.
    public string BillReference { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record TopUpRequest
{
    public Guid FromAccountId { get; init; }
    public string TopUpType { get; init; } = string.Empty; // Mobile, Internet, Gaming
    public string RecipientNumber { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record BeneficiaryDto
{
    public Guid Id { get; init; }
    public Guid OwnerPartyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BeneficiaryType { get; init; } = string.Empty; // IBAN, WalletId, PhoneNumber
    public string Identifier { get; init; } = string.Empty;
    public string? BankName { get; init; }
    public bool IsFavorite { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateBeneficiaryRequest
{
    public Guid OwnerPartyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BeneficiaryType { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string? BankName { get; init; }
}

public record StandingOrderDto
{
    public Guid Id { get; init; }
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string Frequency { get; init; } = string.Empty; // Daily, Weekly, Monthly
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Status { get; init; } = string.Empty; // Active, Paused, Completed, Cancelled
    public DateTime? LastExecutedAt { get; init; }
    public DateTime? NextExecutionAt { get; init; }
}

public record CreateStandingOrderRequest
{
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public string Frequency { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Description { get; init; }
}

public record DirectDebitMandateDto
{
    public Guid Id { get; init; }
    public Guid DebtorAccountId { get; init; }
    public string CreditorName { get; init; } = string.Empty;
    public string CreditorIdentifier { get; init; } = string.Empty;
    public string? IBAN { get; init; }
    public decimal? MaxAmount { get; init; }
    public string Frequency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // Active, Suspended, Cancelled
    public DateTime SignedAt { get; init; }
}

public record CreateDirectDebitMandateRequest
{
    public Guid DebtorAccountId { get; init; }
    public string CreditorName { get; init; } = string.Empty;
    public string CreditorIdentifier { get; init; } = string.Empty;
    public string? IBAN { get; init; }
    public decimal? MaxAmount { get; init; }
    public string Frequency { get; init; } = string.Empty;
}

public record ScheduledPaymentDto
{
    public Guid Id { get; init; }
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public DateTime ScheduledFor { get; init; }
    public string Status { get; init; } = string.Empty; // Pending, Executed, Cancelled, Failed
}

public record SchedulePaymentRequest
{
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public DateTime ScheduledFor { get; init; }
    public string? Description { get; init; }
}
