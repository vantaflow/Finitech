using Finitech.BuildingBlocks.Domain.Integrations;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Integrations;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK PAYMENT] Processing {Amount} {Currency} for {Description}",
            request.AmountMinorUnits / 100m, request.CurrencyCode, request.Description);

        var transactionId = $"TXN-{Guid.NewGuid():N}";

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = transactionId,
            Status = "Approved",
            ProcessedAt = DateTime.UtcNow,
            GatewayReference = transactionId
        });
    }

    public Task<PaymentResult> RefundAsync(string transactionId, long amountMinorUnits, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK REFUND] Refunding {Amount} for transaction {TransactionId}",
            amountMinorUnits / 100m, transactionId);

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = $"REF-{Guid.NewGuid():N}",
            Status = "Refunded",
            ProcessedAt = DateTime.UtcNow,
            GatewayReference = transactionId
        });
    }

    public Task<PaymentStatus> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentStatus
        {
            TransactionId = transactionId,
            Status = "Completed",
            UpdatedAt = DateTime.UtcNow
        });
    }
}
