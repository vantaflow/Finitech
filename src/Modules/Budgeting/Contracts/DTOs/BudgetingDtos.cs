namespace Finitech.Modules.Budgeting.Contracts.DTOs;

public record TransactionCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ParentCategoryId { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}

public record CategorizeTransactionRequest
{
    public Guid TransactionId { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public bool AutoCategorizeSimilar { get; init; }
}

public record BudgetDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountLimitMinorUnits { get; init; }
    public decimal AmountLimitDecimal { get; init; }
    public string Period { get; init; } = string.Empty; // Monthly, Weekly, Yearly
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public long SpentAmountMinorUnits { get; init; }
    public decimal SpentAmountDecimal { get; init; }
    public decimal PercentageUsed { get; init; }
    public bool IsAlertEnabled { get; init; }
    public decimal AlertThresholdPercentage { get; init; }
}

public record CreateBudgetRequest
{
    public Guid PartyId { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public long AmountLimitMinorUnits { get; init; }
    public string Period { get; init; } = "Monthly";
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsAlertEnabled { get; init; } = true;
    public decimal AlertThresholdPercentage { get; init; } = 80m;
}

public record UpdateBudgetRequest
{
    public long? AmountLimitMinorUnits { get; init; }
    public bool? IsAlertEnabled { get; init; }
    public decimal? AlertThresholdPercentage { get; init; }
}

public record SpendingAnalyticsDto
{
    public Guid PartyId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public long TotalSpentMinorUnits { get; init; }
    public decimal TotalSpentDecimal { get; init; }
    public List<CategorySpendingDto> ByCategory { get; init; } = new();
    public List<DailySpendingDto> ByDay { get; init; } = new();
}

public record CategorySpendingDto
{
    public string CategoryId { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public long AmountMinorUnits { get; init; }
    public decimal AmountDecimal { get; init; }
    public decimal PercentageOfTotal { get; init; }
    public int TransactionCount { get; init; }
}

public record DailySpendingDto
{
    public DateTime Date { get; init; }
    public long AmountMinorUnits { get; init; }
    public decimal AmountDecimal { get; init; }
}

public record SpendingTrendDto
{
    public string CategoryId { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public long CurrentPeriodAmountMinorUnits { get; init; }
    public long PreviousPeriodAmountMinorUnits { get; init; }
    public decimal ChangePercentage { get; init; }
    public string Trend { get; init; } = string.Empty; // Increased, Decreased, Stable
}
