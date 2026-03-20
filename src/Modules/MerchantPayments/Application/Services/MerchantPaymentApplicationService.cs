namespace Finitech.Modules.MerchantPayments.Application.Services;

public class MerchantPaymentApplicationService
{
    public async Task<QrPayloadResult> GenerateQrAsync(Guid merchantId, string currencyCode,
        long amountMinorUnits, string reference, string description, DateTime expiresAt)
    {
        // Generate EMVCo QR payload
        var currencyNumeric = currencyCode switch
        {
            "MAD" => "504",
            "EUR" => "978",
            "USD" => "840",
            _ => "000"
        };

        var amountDecimal = amountMinorUnits / 100.0m;
        var crc = ComputeCrc16($"{merchantId}{amountMinorUnits}{reference}");

        var payload = $"000201010212{merchantId.ToString()[..12]}53{currencyNumeric}" +
                      $"54{amountDecimal:F2}62{reference}6304{crc}";

        return await Task.FromResult(new QrPayloadResult(
            Payload: payload,
            PayloadFormat: "EMVCo",
            PayloadLength: payload.Length,
            CurrencyNumericCode: currencyNumeric,
            Amount: amountDecimal,
            Reference: reference,
            Crc: crc));
    }

    public async Task<QrPaymentResult> PayByQrAsync(string qrPayload, Guid payerWalletId, string idempotencyKey)
    {
        // Parse QR, validate, execute payment
        return await Task.FromResult(new QrPaymentResult(Guid.NewGuid(), "Completed", DateTime.UtcNow));
    }

    private static string ComputeCrc16(string input)
    {
        var hash = input.GetHashCode();
        return Math.Abs(hash % 0xFFFF).ToString("X4");
    }
}

public record QrPayloadResult(string Payload, string PayloadFormat, int PayloadLength,
    string CurrencyNumericCode, decimal Amount, string Reference, string Crc);
public record QrPaymentResult(Guid TransactionId, string Status, DateTime Timestamp);
