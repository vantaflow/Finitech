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

    [Theory]
    [InlineData("Maroc Telecom")]
    [InlineData("Inwi")]
    [InlineData("Orange")]
    [InlineData("REDAL")]
    [InlineData("LYDEC")]
    [InlineData("ONEE Eau")]
    [InlineData("ONEE Électricité")]
    [InlineData("Amendis Tanger")]
    [InlineData("Amendis Tétouan")]
    [InlineData("Maroc Telecom Fibre")]
    [InlineData("Direction Générale des Impôts")]
    [InlineData("Trésorerie Générale du Royaume")]
    public async Task PayBill_AllBillers_Work(string billerName)
    {
        var result = await _service.PayBillAsync(Guid.NewGuid(), billerName, "REF-001", "CUST-001", 500);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(billerName, result.BillerName);
    }

    [Fact]
    public void GetSupportedBillers_Returns13()
    {
        var billers = _service.GetSupportedBillers();
        Assert.Equal(13, billers.Count);
    }

    [Fact]
    public void GetBillersByCategory_HasAllCategories()
    {
        var categories = _service.GetSupportedBillersByCategory();
        Assert.True(categories.ContainsKey("Telecom"));
        Assert.True(categories.ContainsKey("Eau"));
        Assert.True(categories.ContainsKey("Électricité"));
        Assert.True(categories.ContainsKey("Taxes & Gouvernement"));
    }

    [Fact]
    public async Task VerifyBill_EmptyRef_Fails()
    {
        var result = await _service.VerifyBillAsync("Maroc Telecom", "", "CUST-001", 500);
        Assert.False(result.IsValid);
    }
}
