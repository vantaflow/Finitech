using Finitech.Modules.Budgeting.Contracts.DTOs;

namespace Finitech.Modules.Budgeting.Contracts;

public interface IBudgetingService
{
    Task CategorizeTransactionAsync(CategorizeTransactionRequest request, CancellationToken cancellationToken = default);
    Task<TransactionCategoryDto?> AutoCategorizeAsync(Guid transactionId, string description, string? merchantName, CancellationToken cancellationToken = default);

    Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken = default);
    Task<BudgetDto?> GetBudgetAsync(Guid budgetId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BudgetDto>> GetBudgetsByPartyAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken = default);
    Task DeleteBudgetAsync(Guid budgetId, CancellationToken cancellationToken = default);

    Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(Guid partyId, string currencyCode, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpendingTrendDto>> GetSpendingTrendsAsync(Guid partyId, string currencyCode, string period, CancellationToken cancellationToken = default);
    Task CheckBudgetAlertsAsync(Guid partyId, CancellationToken cancellationToken = default);
}
