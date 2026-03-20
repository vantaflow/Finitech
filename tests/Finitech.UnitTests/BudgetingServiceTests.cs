using Finitech.Modules.Budgeting.Application.Services;
using Xunit;

namespace Finitech.UnitTests;

public class BudgetingServiceTests
{
    private readonly BudgetingApplicationService _service = new();

    [Fact]
    public async Task CreateBudget_ValidRequest_ReturnsNewBudget()
    {
        var partyId = Guid.NewGuid();
        var result = await _service.CreateBudgetAsync(
            partyId, "restaurants", "MAD", 200000, "Monthly", true, 80);

        Assert.Equal(partyId, result.PartyId);
        Assert.Equal("restaurants", result.CategoryId);
        Assert.Equal(200000, result.AmountLimitMinorUnits);
        Assert.Equal(0, result.SpentMinorUnits);
        Assert.True(result.AlertEnabled);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsAnalytics()
    {
        var result = await _service.GetAnalyticsAsync(
            Guid.NewGuid(), "MAD", DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        Assert.NotNull(result);
        Assert.NotNull(result.Categories);
    }
}
