using Finitech.Modules.FX.Contracts;
using Finitech.Modules.FX.Contracts.DTOs;
using System.Collections.Concurrent;

namespace Finitech.ApiHost.Services;

public class FXService : IFXService
{
    private readonly ConcurrentDictionary<(string From, string To), decimal> _rates = new();
    private readonly ConcurrentDictionary<Guid, FXQuoteResponse> _quotes = new();
    private readonly ConcurrentDictionary<string, FXConvertResponse> _idempotencyKeys = new();

    public FXService()
    {
        // Seed fixed rates
        _rates[("MAD", "EUR")] = 0.091m;
        _rates[("EUR", "MAD")] = 11.0m;
        _rates[("MAD", "USD")] = 0.10m;
        _rates[("USD", "MAD")] = 10.0m;
        _rates[("EUR", "USD")] = 1.1m;
        _rates[("USD", "EUR")] = 0.91m;
    }

    public Task<FXRateDto> GetRateAsync(FXRateRequest request, CancellationToken cancellationToken = default)
    {
        var rate = _rates.GetValueOrDefault((request.FromCurrencyCode, request.ToCurrencyCode), 1m);

        return Task.FromResult(new FXRateDto
        {
            FromCurrencyCode = request.FromCurrencyCode,
            ToCurrencyCode = request.ToCurrencyCode,
            Rate = rate,
            InverseRate = rate > 0 ? 1 / rate : 0,
            EffectiveAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
    }

    public Task<FXQuoteResponse> GetQuoteAsync(FXQuoteRequest request, CancellationToken cancellationToken = default)
    {
        var rate = _rates.GetValueOrDefault((request.FromCurrencyCode, request.ToCurrencyCode), 1m);
        var feeRate = 0.005m; // 0.5% fee

        var sourceAmount = request.AmountMinorUnits / 100m;
        var targetAmount = sourceAmount * rate;
        var fee = targetAmount * feeRate;
        var netAmount = targetAmount - fee;

        var quote = new FXQuoteResponse
        {
            QuoteId = Guid.NewGuid(),
            FromCurrencyCode = request.FromCurrencyCode,
            ToCurrencyCode = request.ToCurrencyCode,
            SourceAmountMinorUnits = request.AmountMinorUnits,
            SourceAmountDecimal = sourceAmount,
            TargetAmountMinorUnits = (long)(netAmount * 100),
            TargetAmountDecimal = netAmount,
            Rate = rate,
            FeeMinorUnits = (long)(fee * 100),
            FeeDecimal = fee,
            NetAmountMinorUnits = (long)(netAmount * 100),
            NetAmountDecimal = netAmount,
            ValidUntil = DateTime.UtcNow.AddMinutes(5)
        };

        _quotes[quote.QuoteId] = quote;
        return Task.FromResult(quote);
    }

    public Task<FXConvertResponse> ConvertAsync(FXConvertRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IdempotencyKey != null && _idempotencyKeys.TryGetValue(request.IdempotencyKey, out var existing))
        {
            return Task.FromResult(existing);
        }

        if (!_quotes.TryGetValue(request.QuoteId, out var quote))
            throw new InvalidOperationException("Quote not found or expired");

        if (quote.ValidUntil < DateTime.UtcNow)
            throw new InvalidOperationException("Quote expired");

        var response = new FXConvertResponse
        {
            ConversionId = Guid.NewGuid(),
            Status = "Completed",
            SourceLedgerEntryId = Guid.NewGuid(),
            TargetLedgerEntryId = Guid.NewGuid(),
            FeeLedgerEntryId = quote.FeeMinorUnits > 0 ? Guid.NewGuid() : null
        };

        if (request.IdempotencyKey != null)
        {
            _idempotencyKeys[request.IdempotencyKey] = response;
        }

        return Task.FromResult(response);
    }
}
