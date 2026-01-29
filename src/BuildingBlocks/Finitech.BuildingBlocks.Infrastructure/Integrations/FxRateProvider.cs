using Finitech.BuildingBlocks.Domain.Integrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Integrations;

public class FxRateProvider : IFxRateProvider
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<FxRateProvider> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private static readonly Dictionary<string, decimal> MockRates = new()
    {
        ["MAD"] = 1.0m,
        ["EUR"] = 0.091m,
        ["USD"] = 0.098m,
        ["GBP"] = 0.077m
    };

    public FxRateProvider(IMemoryCache cache, ILogger<FxRateProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<FxRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"fx:{fromCurrency}:{toCurrency}";

        if (_cache.TryGetValue(cacheKey, out FxRate? cachedRate) && cachedRate != null)
        {
            return Task.FromResult(cachedRate);
        }

        var rate = CalculateCrossRate(fromCurrency, toCurrency);
        var fxRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            Rate = rate,
            InverseRate = 1 / rate,
            Timestamp = DateTime.UtcNow
        };

        _cache.Set(cacheKey, fxRate, CacheDuration);

        return Task.FromResult(fxRate);
    }

    public Task<IReadOnlyList<FxRate>> GetAllRatesAsync(CancellationToken cancellationToken = default)
    {
        var rates = new List<FxRate>();
        var currencies = MockRates.Keys.ToList();

        foreach (var from in currencies)
        {
            foreach (var to in currencies)
            {
                if (from != to)
                {
                    rates.Add(new FxRate
                    {
                        FromCurrency = from,
                        ToCurrency = to,
                        Rate = CalculateCrossRate(from, to),
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }

        return Task.FromResult<IReadOnlyList<FxRate>>(rates);
    }

    private static decimal CalculateCrossRate(string from, string to)
    {
        if (!MockRates.TryGetValue(from, out var fromRate) || !MockRates.TryGetValue(to, out var toRate))
        {
            throw new ArgumentException("Currency not supported");
        }

        return toRate / fromRate;
    }
}
