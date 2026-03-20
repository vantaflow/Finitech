namespace Finitech.Modules.Disputes.Application.Services;

public class DisputeApplicationService
{
    public Task<RefundResult> ProcessRefundAsync(Guid originalTransactionId, long amountMinorUnits, string reason, string idempotencyKey)
        => Task.FromResult(new RefundResult(Guid.NewGuid(), "Completed", DateTime.UtcNow));

    public Task<ChargebackResult> InitiateChargebackAsync(Guid originalTransactionId, string reason, string evidenceDescription)
        => Task.FromResult(new ChargebackResult(Guid.NewGuid(), "Initiated", DateTime.UtcNow));
}

public record RefundResult(Guid RefundId, string Status, DateTime Timestamp);
public record ChargebackResult(Guid ChargebackId, string Status, DateTime Timestamp);
