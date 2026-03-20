namespace Finitech.Modules.Budgeting.Application.Services;

public class BudgetingApplicationService
{
    public async Task<BudgetDto> CreateBudgetAsync(Guid partyId, string categoryId, string currencyCode,
        long amountLimitMinorUnits, string period, bool alertEnabled, int alertThresholdPercentage)
    {
        var budget = new BudgetDto(
            Id: Guid.NewGuid(),
            PartyId: partyId,
            CategoryId: categoryId,
            CurrencyCode: currencyCode,
            AmountLimitMinorUnits: amountLimitMinorUnits,
            SpentMinorUnits: 0,
            Period: period,
            AlertEnabled: alertEnabled,
            AlertThresholdPercentage: alertThresholdPercentage);

        return await Task.FromResult(budget);
    }

    public async Task<SpendingAnalyticsDto> GetAnalyticsAsync(Guid partyId, string currencyCode, DateTime fromDate, DateTime toDate)
    {
        // Aggregate spending by category for the period
        return await Task.FromResult(new SpendingAnalyticsDto(
            PartyId: partyId,
            Period: $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
            TotalSpentMinorUnits: 0,
            Categories: new List<CategorySpending>()));
    }
}

public record BudgetDto(Guid Id, Guid PartyId, string CategoryId, string CurrencyCode,
    long AmountLimitMinorUnits, long SpentMinorUnits, string Period,
    bool AlertEnabled, int AlertThresholdPercentage);
public record SpendingAnalyticsDto(Guid PartyId, string Period, long TotalSpentMinorUnits, List<CategorySpending> Categories);
public record CategorySpending(string CategoryId, long AmountMinorUnits, decimal Percentage);
