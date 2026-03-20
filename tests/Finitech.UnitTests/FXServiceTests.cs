using Finitech.Modules.FX.Application.Services;
using Xunit;

namespace Finitech.UnitTests;

public class FXServiceTests
{
    private readonly FXApplicationService _service = new();

    [Theory]
    [InlineData("MAD", "EUR")]
    [InlineData("EUR", "MAD")]
    [InlineData("MAD", "USD")]
    [InlineData("USD", "EUR")]
    public async Task GetRate_SupportedPairs_ReturnsPositiveRate(string from, string to)
    {
        var result = await _service.GetRateAsync(from, to);
        Assert.True(result.Rate > 0);
        Assert.Equal(from, result.FromCurrency);
        Assert.Equal(to, result.ToCurrency);
    }

    [Fact]
    public async Task CreateQuote_ValidPair_ReturnsConvertedAmount()
    {
        var result = await _service.CreateQuoteAsync("MAD", "EUR", 100000);
        Assert.True(result.ConvertedAmount > 0);
        Assert.True(result.ConvertedAmount < 100000); // MAD→EUR should be less
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateQuote_SameCurrency_ReturnsEqualAmount()
    {
        var result = await _service.CreateQuoteAsync("MAD", "MAD", 50000);
        Assert.Equal(50000, result.ConvertedAmount);
    }
}
