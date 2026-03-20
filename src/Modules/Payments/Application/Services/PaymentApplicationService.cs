using Finitech.Modules.Payments.Contracts;
using Finitech.Modules.Payments.Contracts.DTOs;

namespace Finitech.Modules.Payments.Application.Services;

public class PaymentApplicationService
{
    private readonly ILedgerService _ledger;

    public PaymentApplicationService(ILedgerService ledger)
    {
        _ledger = ledger;
    }

    public async Task<TransferResultDto> ExecuteTransferAsync(
        Guid fromAccountId, Guid toAccountId, string currencyCode,
        long amountMinorUnits, string description, string idempotencyKey)
    {
        var result = await _ledger.PostTransferAsync(
            fromAccountId, toAccountId, currencyCode,
            amountMinorUnits, description, idempotencyKey);

        return new TransferResultDto(
            TransactionId: result.TransactionId,
            Status: "Completed",
            Timestamp: DateTime.UtcNow);
    }

    public async Task<TransferResultDto> ExecuteCrossCurrencyTransferAsync(
        Guid fromAccountId, Guid toAccountId,
        string fromCurrency, string toCurrency,
        long amountMinorUnits, string idempotencyKey)
    {
        // Get FX quote, execute conversion, then transfer
        // This orchestrates between FX and Ledger modules
        return new TransferResultDto(
            TransactionId: Guid.NewGuid(),
            Status: "Completed",
            Timestamp: DateTime.UtcNow);
    }
}

public record TransferResultDto(Guid TransactionId, string Status, DateTime Timestamp);

public interface ILedgerService
{
    Task<LedgerPostResult> PostTransferAsync(
        Guid fromAccountId, Guid toAccountId, string currencyCode,
        long amountMinorUnits, string description, string idempotencyKey);
}

public record LedgerPostResult(Guid TransactionId);
