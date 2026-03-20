namespace Finitech.Modules.Budgeting.Application.Services;

public class BudgetingApplicationService
{
    public Task<BudgetDto> CreateBudgetAsync(Guid partyId, string categoryId, string currencyCode, long amountLimitMinorUnits, string period, bool alertEnabled, int alertThresholdPercentage)
        => Task.FromResult(new BudgetDto(Guid.NewGuid(), partyId, categoryId, currencyCode, amountLimitMinorUnits, 0, period, alertEnabled, alertThresholdPercentage));

    public Task<SpendingAnalyticsDto> GetAnalyticsAsync(Guid partyId, string currencyCode, DateTime fromDate, DateTime toDate)
        => Task.FromResult(new SpendingAnalyticsDto(partyId, $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", 0, new List<CategorySpending>()));
}

public record BudgetDto(Guid Id, Guid PartyId, string CategoryId, string CurrencyCode, long AmountLimitMinorUnits, long SpentMinorUnits, string Period, bool AlertEnabled, int AlertThresholdPercentage);
public record SpendingAnalyticsDto(Guid PartyId, string Period, long TotalSpentMinorUnits, List<CategorySpending> Categories);
public record CategorySpending(string CategoryId, long AmountMinorUnits, decimal Percentage);
