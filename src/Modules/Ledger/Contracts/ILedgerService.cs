using Finitech.Modules.Ledger.Contracts.DTOs;

namespace Finitech.Modules.Ledger.Contracts;

public interface ILedgerService
{
    Task<GetBalancesResponse> GetBalancesAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<BalanceDto?> GetBalanceAsync(Guid accountId, string currencyCode, CancellationToken cancellationToken = default);
    Task<GetHistoryResponse> GetHistoryAsync(Guid accountId, GetHistoryRequest request, CancellationToken cancellationToken = default);
    Task<PostTransactionResponse> PostTransactionAsync(PostTransactionRequest request, CancellationToken cancellationToken = default);
    Task<PostTransactionResponse> VoidTransactionAsync(VoidTransactionRequest request, CancellationToken cancellationToken = default);
    Task<PostTransactionResponse> ApplyAdjustmentAsync(AdjustmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> HasSufficientBalanceAsync(Guid accountId, string currencyCode, long amountMinorUnits, CancellationToken cancellationToken = default);
}
