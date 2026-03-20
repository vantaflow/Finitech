namespace Finitech.Modules.FX.Application.Services;

public class FXApplicationService
{
    public async Task<FxRateResult> GetRateAsync(string fromCurrency, string toCurrency)
    {
        // In production, this calls an external FX provider
        var rate = GetSimulatedRate(fromCurrency, toCurrency);
        return await Task.FromResult(new FxRateResult(fromCurrency, toCurrency, rate, DateTime.UtcNow));
    }

    public async Task<FxQuoteResult> CreateQuoteAsync(string fromCurrency, string toCurrency, long amountMinorUnits)
    {
        var rate = GetSimulatedRate(fromCurrency, toCurrency);
        var convertedAmount = (long)(amountMinorUnits * rate);
        var quoteId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        return await Task.FromResult(new FxQuoteResult(quoteId, rate, amountMinorUnits, convertedAmount, expiresAt));
    }

    private static decimal GetSimulatedRate(string from, string to)
    {
        return (from, to) switch
        {
            ("MAD", "EUR") => 0.092m,
            ("EUR", "MAD") => 10.87m,
            ("MAD", "USD") => 0.099m,
            ("USD", "MAD") => 10.10m,
            ("EUR", "USD") => 1.08m,
            ("USD", "EUR") => 0.93m,
            _ => 1.0m
        };
    }
}

public record FxRateResult(string FromCurrency, string ToCurrency, decimal Rate, DateTime Timestamp);
public record FxQuoteResult(Guid QuoteId, decimal Rate, long OriginalAmount, long ConvertedAmount, DateTime ExpiresAt);
