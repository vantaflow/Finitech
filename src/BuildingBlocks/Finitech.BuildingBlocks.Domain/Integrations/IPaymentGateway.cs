namespace Finitech.BuildingBlocks.Domain.Integrations;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> RefundAsync(string transactionId, long amountMinorUnits, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default);
}

public class PaymentRequest
{
    public long AmountMinorUnits { get; set; }
    public string CurrencyCode { get; set; } = "MAD";
    public string Description { get; set; } = string.Empty;
    public string CardToken { get; set; } = string.Empty;
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string GatewayReference { get; set; } = string.Empty;
}

public class PaymentStatus
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
