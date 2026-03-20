using Finitech.Modules.Wallet.Application.Services;
using Finitech.Modules.Wallet.Domain.Enums;
using Xunit;

namespace Finitech.UnitTests;

public class FraudDetectionTests
{
    private readonly FraudDetectionService _service = new();

    [Fact]
    public async Task Deposit_SmallAmount_IsClean()
    {
        var result = await _service.CheckDepositAsync(Guid.NewGuid(), 500, PaymentMethod.CreditCard);
        Assert.True(result.IsClean);
        Assert.Empty(result.Flags);
    }

    [Fact]
    public async Task Deposit_LargeAmount_Flagged()
    {
        var result = await _service.CheckDepositAsync(Guid.NewGuid(), 15000, PaymentMethod.BankTransfer);
        Assert.False(result.IsClean);
        Assert.Contains(result.Flags, f => f.Contains("10,000"));
    }

    [Fact]
    public async Task Deposit_LargeCash_DoubleFlagged()
    {
        var result = await _service.CheckDepositAsync(Guid.NewGuid(), 12000, PaymentMethod.Cash);
        Assert.False(result.IsClean);
        Assert.True(result.Flags.Count >= 2);
    }

    [Fact]
    public async Task Transfer_ToSelf_Flagged()
    {
        var walletId = Guid.NewGuid();
        var result = await _service.CheckTransferAsync(walletId, walletId, 100);
        Assert.False(result.IsClean);
        Assert.Contains(result.Flags, f => f.Contains("soi-même"));
    }
}
