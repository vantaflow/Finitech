namespace Finitech.Modules.MerchantPayments.Application.Services;

public class MerchantPaymentApplicationService
{
    public Task<QrPayloadResult> GenerateQrAsync(Guid merchantId, string currencyCode, long amountMinorUnits, string reference, string description, DateTime expiresAt)
    {
        var currencyNumeric = currencyCode switch { "MAD" => "504", "EUR" => "978", "USD" => "840", _ => "000" };
        var amountDecimal = amountMinorUnits / 100.0m;
        var crc = Math.Abs($"{merchantId}{amountMinorUnits}{reference}".GetHashCode() % 0xFFFF).ToString("X4");
        var payload = $"000201010212{merchantId.ToString()[..12]}53{currencyNumeric}54{amountDecimal:F2}62{reference}6304{crc}";

        return Task.FromResult(new QrPayloadResult(payload, "EMVCo", payload.Length, currencyNumeric, amountDecimal, reference, crc));
    }

    public Task<QrPaymentResult> PayByQrAsync(string qrPayload, Guid payerWalletId, string idempotencyKey)
        => Task.FromResult(new QrPaymentResult(Guid.NewGuid(), "Completed", DateTime.UtcNow));
}

public record QrPayloadResult(string Payload, string PayloadFormat, int PayloadLength, string CurrencyNumericCode, decimal Amount, string Reference, string Crc);
public record QrPaymentResult(Guid TransactionId, string Status, DateTime Timestamp);
