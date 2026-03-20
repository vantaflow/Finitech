using Finitech.Modules.FX.Domain.Entities;

namespace Finitech.Modules.FX.Application.Services;

/// <summary>
/// FX deal service with risk checks, commission calculation and T+2 settlement.
/// Merged from Bankin — professional-grade FX trading.
/// </summary>
public class FXDealService
{
    private readonly FXApplicationService _fxService = new();

    public async Task<FXQuote> CreateQuoteAsync(string baseCurrency, string quoteCurrency, decimal amount)
    {
        var fxResult = await _fxService.GetRateAsync(baseCurrency, quoteCurrency);
        var spread = fxResult.Rate * 0.005m; // 0.5% spread

        return new FXQuote
        {
            Id = Guid.NewGuid(),
            QuoteId = $"Q-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}",
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency,
            MidRate = fxResult.Rate,
            BidRate = fxResult.Rate - spread / 2,
            AskRate = fxResult.Rate + spread / 2,
            Spread = spread,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMinutes(5),
            MinAmount = 100,
            MaxAmount = 1000000,
            Source = "ECB"
        };
    }

    public Task<FXDeal> ExecuteDealAsync(FXQuote quote, decimal buyAmount, string customerId, string idempotencyKey)
    {
        if (!quote.IsValid())
            throw new InvalidOperationException("Quote has expired");

        var rate = quote.GetRate(isBuy: true);
        var sellAmount = buyAmount * rate;
        var commission = sellAmount * 0.001m; // 0.1% commission

        var deal = new FXDeal
        {
            Id = Guid.NewGuid(),
            DealNumber = $"FX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}",
            QuoteId = quote.Id,
            BuyCurrency = quote.BaseCurrency,
            SellCurrency = quote.QuoteCurrency,
            BuyAmount = buyAmount,
            SellAmount = sellAmount,
            ExchangeRate = rate,
            TradeDate = DateTime.UtcNow,
            ValueDate = GetT2SettlementDate(),
            CustomerId = customerId,
            IdempotencyKey = idempotencyKey,
            Commission = commission,
            CommissionCurrency = quote.QuoteCurrency,
            Status = "Executed"
        };

        return Task.FromResult(deal);
    }

    private static DateTime GetT2SettlementDate()
    {
        var date = DateTime.UtcNow.AddDays(2);
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(1);
        return date;
    }
}
