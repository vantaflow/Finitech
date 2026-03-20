using Finitech.Modules.Wallet.Application.Services;
using Finitech.Modules.Wallet.Domain.Enums;
using Xunit;

namespace Finitech.UnitTests;

public class KycServiceTests
{
    private readonly KycApplicationService _service = new();

    [Theory]
    [InlineData("AB123456", true)]
    [InlineData("A12345", true)]
    [InlineData("BK678901", true)]
    [InlineData("", false)]
    [InlineData("12345", false)]
    [InlineData("ABCDEF", false)]
    public async Task BasicVerification_CinFormat_ValidatesCorrectly(string cin, bool expectedSuccess)
    {
        var result = await _service.InitiateBasicVerificationAsync(Guid.NewGuid(), cin);
        Assert.Equal(expectedSuccess, result.Success);
    }

    [Fact]
    public async Task FullVerification_Under18_Rejected()
    {
        var result = await _service.VerifyIdentityAsync(Guid.NewGuid(), "AB123456", "Ahmed",
            DateTime.UtcNow.AddYears(-15), "front.jpg", "back.jpg", "selfie.jpg");
        Assert.False(result.Success);
    }

    [Fact]
    public async Task FullVerification_ValidDocuments_ReturnsStandard()
    {
        var result = await _service.VerifyIdentityAsync(Guid.NewGuid(), "AB123456", "Ahmed Benali",
            DateTime.UtcNow.AddYears(-25), "front.jpg", "back.jpg", "selfie.jpg");
        Assert.True(result.Success);
        Assert.Equal(KycLevel.Standard, result.Level);
    }

    [Fact]
    public async Task AdvancedUpgrade_WithoutIdentity_Rejected()
    {
        var result = await _service.UpgradeToAdvancedAsync(Guid.NewGuid(), isIdentityVerified: false);
        Assert.False(result.Success);
    }

    [Theory]
    [InlineData(KycLevel.None, 0, 0)]
    [InlineData(KycLevel.Basic, 2000, 10000)]
    [InlineData(KycLevel.Standard, 10000, 50000)]
    [InlineData(KycLevel.Advanced, 20000, 100000)]
    public void GetLimits_ReturnsCorrectLimits(KycLevel level, decimal daily, decimal monthly)
    {
        var limits = _service.GetLimitsForLevel(level);
        Assert.Equal(daily, limits.DailyLimitMAD);
        Assert.Equal(monthly, limits.MonthlyLimitMAD);
    }
}
