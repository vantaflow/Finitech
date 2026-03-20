using Finitech.Modules.Wallet.Application.Services;
using Finitech.Modules.Wallet.Domain.Enums;
using Xunit;

namespace Finitech.UnitTests;

public class OfferServiceTests
{
    private readonly OfferApplicationService _service = new();

    [Fact]
    public async Task CreateOffer_Cashback_ValidPercentage_Succeeds()
    {
        var offer = await _service.CreateOfferAsync("Cashback 5%", "5% sur tous les achats",
            OfferType.Cashback, 50000, DateTime.UtcNow, DateTime.UtcNow.AddMonths(3), cashbackPct: 5);
        Assert.Equal(OfferType.Cashback, offer.Type);
        Assert.True(offer.IsActive);
    }

    [Fact]
    public async Task CreateOffer_InvalidDates_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateOfferAsync("Test", "Desc", OfferType.Cashback, 1000,
                DateTime.UtcNow.AddDays(10), DateTime.UtcNow, cashbackPct: 5));
    }

    [Fact]
    public async Task CreateOffer_CashbackOver100_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateOfferAsync("Test", "Desc", OfferType.Cashback, 1000,
                DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), cashbackPct: 150));
    }

    [Fact]
    public async Task DeactivateOffer_SetsInactive()
    {
        var offer = await _service.CreateOfferAsync("Test", "Desc", OfferType.LoyaltyProgram,
            5000, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        await _service.DeactivateOfferAsync(offer);
        Assert.False(offer.IsActive);
    }
}
