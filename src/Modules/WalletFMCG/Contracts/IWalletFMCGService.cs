using Finitech.Modules.WalletFMCG.Contracts.DTOs;

namespace Finitech.Modules.WalletFMCG.Contracts;

public interface IWalletFMCGService
{
    Task<FloatAccountDto> CreateFloatAccountAsync(Guid partyId, string accountType, CancellationToken cancellationToken = default);
    Task<FloatAccountDto?> GetFloatAccountAsync(Guid floatAccountId, CancellationToken cancellationToken = default);
    Task<FloatAccountDto?> GetFloatAccountByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default);

    Task<CashOperationResultDto> CashInAsync(CashInRequest request, CancellationToken cancellationToken = default);
    Task<CashOperationResultDto> CashOutAsync(CashOutRequest request, CancellationToken cancellationToken = default);

    Task<NetworkHierarchyDto> GetNetworkHierarchyAsync(Guid distributorId, CancellationToken cancellationToken = default);
    Task AssignAgentToDistributorAsync(Guid agentId, Guid distributorId, CancellationToken cancellationToken = default);
    Task AssignMerchantToAgentAsync(Guid merchantId, Guid agentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommissionDto>> GetCommissionsAsync(Guid beneficiaryId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<CommissionDto> CalculateCommissionAsync(Guid beneficiaryId, string beneficiaryType, string operationType, string currencyCode, long amountMinorUnits, Guid originalTransactionId, CancellationToken cancellationToken = default);

    Task SetCommissionRuleAsync(CommissionRuleDto rule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommissionRuleDto>> GetCommissionRulesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FloatAlertDto>> GetFloatAlertsAsync(Guid floatAccountId, CancellationToken cancellationToken = default);
}
