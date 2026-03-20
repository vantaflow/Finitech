using Finitech.Modules.MerchantPayments.Application.Services;
using Xunit;

namespace Finitech.UnitTests;

public class MerchantPaymentTests
{
    private readonly MerchantPaymentApplicationService _service = new();

    [Fact]
    public async Task GenerateQr_MAD_Returns504CurrencyCode()
    {
        var result = await _service.GenerateQrAsync(
            Guid.NewGuid(), "MAD", 15000, "CMD-001", "Café", DateTime.UtcNow.AddMinutes(30));

        Assert.Equal("EMVCo", result.PayloadFormat);
        Assert.Equal("504", result.CurrencyNumericCode);
        Assert.Equal(150.00m, result.Amount);
        Assert.False(string.IsNullOrEmpty(result.Crc));
    }

    [Theory]
    [InlineData("MAD", "504")]
    [InlineData("EUR", "978")]
    [InlineData("USD", "840")]
    public async Task GenerateQr_MultipleCurrencies_CorrectNumericCode(string currency, string expected)
    {
        var result = await _service.GenerateQrAsync(
            Guid.NewGuid(), currency, 10000, "REF-001", "Test", DateTime.UtcNow.AddMinutes(5));

        Assert.Equal(expected, result.CurrencyNumericCode);
    }

    [Fact]
    public async Task PayByQr_ValidPayload_ReturnsCompleted()
    {
        var result = await _service.PayByQrAsync("000201...", Guid.NewGuid(), "pay-001");
        Assert.Equal("Completed", result.Status);
    }
}
