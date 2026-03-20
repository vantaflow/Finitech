using Finitech.Modules.FX.Application.Services;
using Xunit;

namespace Finitech.UnitTests;

public class FXDealTests
{
    private readonly FXDealService _service = new();

    [Fact]
    public async Task CreateQuote_MADtoEUR_HasBidAskSpread()
    {
        var quote = await _service.CreateQuoteAsync("MAD", "EUR", 10000);
        Assert.True(quote.BidRate > 0);
        Assert.True(quote.AskRate > quote.BidRate); // Ask > Bid
        Assert.True(quote.Spread > 0);
        Assert.True(quote.IsValid());
    }

    [Fact]
    public async Task ExecuteDeal_ValidQuote_ReturnsExecutedDeal()
    {
        var quote = await _service.CreateQuoteAsync("MAD", "EUR", 10000);
        var deal = await _service.ExecuteDealAsync(quote, 10000, "CUST-001", "idem-001");

        Assert.Equal("Executed", deal.Status);
        Assert.Equal("MAD", deal.BuyCurrency);
        Assert.Equal("EUR", deal.SellCurrency);
        Assert.True(deal.Commission > 0);
        Assert.True(deal.ValueDate > deal.TradeDate); // T+2
        Assert.DoesNotContain("Saturday", deal.ValueDate.DayOfWeek.ToString());
        Assert.DoesNotContain("Sunday", deal.ValueDate.DayOfWeek.ToString());
    }

    [Fact]
    public async Task ExecuteDeal_ExpiredQuote_Throws()
    {
        var quote = await _service.CreateQuoteAsync("MAD", "EUR", 10000);
        quote.ValidTo = DateTime.UtcNow.AddMinutes(-1); // Force expiry
        quote.Status = "Expired";

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ExecuteDealAsync(quote, 10000, "CUST-001", "idem-002"));
    }
}
