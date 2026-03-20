using Finitech.Modules.Wallet.Application.Services;
using Xunit;

namespace Finitech.UnitTests;

public class BillPaymentTests
{
    private readonly BillPaymentApplicationService _service = new();

    [Fact]
    public async Task VerifyBill_ValidBiller_Succeeds()
    {
        var result = await _service.VerifyBillAsync("Maroc Telecom", "REF-001", "CUST-001", 500);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VerifyBill_UnsupportedBiller_Fails()
    {
        var result = await _service.VerifyBillAsync("UnknownCompany", "REF-001", "CUST-001", 500);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task VerifyBill_EmptyData_Fails()
    {
        var result = await _service.VerifyBillAsync("", "", "", 0);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Maroc Telecom")]
    [InlineData("Inwi")]
    [InlineData("Orange")]
    [InlineData("REDAL")]
    [InlineData("LYDEC")]
    [InlineData("ONEE")]
    public async Task PayBill_SupportedBillers_AllWork(string billerName)
    {
        var result = await _service.PayBillAsync(Guid.NewGuid(), billerName, "REF-001", "CUST-001", 500);
        Assert.Equal("Completed", result.Status);
        Assert.False(string.IsNullOrEmpty(result.TransactionReference));
    }

    [Fact]
    public void GetSupportedBillers_Returns7()
    {
        var billers = _service.GetSupportedBillers();
        Assert.True(billers.Count >= 7);
    }
}
