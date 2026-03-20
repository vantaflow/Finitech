namespace Finitech.Modules.Payments.Application.Services;

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessTransferAsync(Guid fromAccountId, Guid toAccountId, string currencyCode, long amountMinorUnits, string description, string idempotencyKey);
}

public class PaymentApplicationService
{
    private readonly IPaymentProcessor _processor;

    public PaymentApplicationService(IPaymentProcessor processor) => _processor = processor;

    public async Task<PaymentResult> ExecuteTransferAsync(Guid fromAccountId, Guid toAccountId, string currencyCode, long amountMinorUnits, string description, string idempotencyKey)
    {
        if (amountMinorUnits <= 0)
            return new PaymentResult(Guid.Empty, "Failed", "Amount must be positive");

        return await _processor.ProcessTransferAsync(fromAccountId, toAccountId, currencyCode, amountMinorUnits, description, idempotencyKey);
    }
}

public record PaymentResult(Guid TransactionId, string Status, string? Message = null);
