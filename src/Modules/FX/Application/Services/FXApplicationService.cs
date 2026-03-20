namespace Finitech.Modules.FX.Application.Services;

public class FXApplicationService
{
    public Task<FxRateResult> GetRateAsync(string fromCurrency, string toCurrency)
    {
        var rate = GetSimulatedRate(fromCurrency, toCurrency);
        return Task.FromResult(new FxRateResult(fromCurrency, toCurrency, rate, DateTime.UtcNow));
    }

    public Task<FxQuoteResult> CreateQuoteAsync(string fromCurrency, string toCurrency, long amountMinorUnits)
    {
        var rate = GetSimulatedRate(fromCurrency, toCurrency);
        var convertedAmount = (long)(amountMinorUnits * rate);
        return Task.FromResult(new FxQuoteResult(Guid.NewGuid(), rate, amountMinorUnits, convertedAmount, DateTime.UtcNow.AddMinutes(5)));
    }

    private static decimal GetSimulatedRate(string from, string to) => (from, to) switch
    {
        ("MAD", "EUR") => 0.092m, ("EUR", "MAD") => 10.87m,
        ("MAD", "USD") => 0.099m, ("USD", "MAD") => 10.10m,
        ("EUR", "USD") => 1.08m,  ("USD", "EUR") => 0.93m,
        _ => 1.0m
    };
}

public record FxRateResult(string FromCurrency, string ToCurrency, decimal Rate, DateTime Timestamp);
public record FxQuoteResult(Guid QuoteId, decimal Rate, long OriginalAmount, long ConvertedAmount, DateTime ExpiresAt);
