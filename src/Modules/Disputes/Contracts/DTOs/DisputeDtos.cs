namespace Finitech.Modules.Disputes.Contracts.DTOs;

public record RefundRequest
{
    public Guid OriginalTransactionId { get; init; }
    public long? AmountMinorUnits { get; init; } // null = full refund
    public string Reason { get; init; } = string.Empty;
    public string? IdempotencyKey { get; init; }
}

public record RefundResponse
{
    public Guid RefundId { get; init; }
    public Guid LedgerEntryId { get; init; }
    public string Status { get; init; } = string.Empty;
    public long RefundedAmountMinorUnits { get; init; }
    public DateTime ExecutedAt { get; init; }
}

public record ChargebackRequest
{
    public Guid OriginalTransactionId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? EvidenceDescription { get; init; }
}

public record ChargebackDto
{
    public Guid Id { get; init; }
    public Guid OriginalTransactionId { get; init; }
    public string Status { get; init; } = string.Empty; // Initiated, Represented, Won, Lost
    public string Reason { get; init; } = string.Empty;
    public DateTime InitiatedAt { get; init; }
    public DateTime? RepresentedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? Resolution { get; init; }
}

public record RepresentmentRequest
{
    public Guid ChargebackId { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public List<string> EvidenceDocumentUrls { get; init; } = new();
}

public record RepresentmentDto
{
    public Guid Id { get; init; }
    public Guid ChargebackId { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public List<string> EvidenceDocumentUrls { get; init; } = new();
    public DateTime SubmittedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record ResolveChargebackRequest
{
    public Guid ChargebackId { get; init; }
    public string Resolution { get; init; } = string.Empty; // Won, Lost
    public string? Notes { get; init; }
}
