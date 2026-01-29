using Finitech.Modules.Wallet.Contracts.DTOs;

namespace Finitech.Modules.Wallet.Contracts;

public interface IWalletService
{
    Task<WalletAccountDto> CreateWalletAsync(CreateWalletRequest request, CancellationToken cancellationToken = default);
    Task<WalletAccountDto?> GetWalletAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<WalletAccountDto?> GetWalletByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WalletLimitsDto>> GetWalletLimitsAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<TransferResultDto> P2PSendAsync(P2PSendRequest request, CancellationToken cancellationToken = default);
    Task<P2PRequestDto> P2PRequestMoneyAsync(P2PRequestMoneyRequest request, CancellationToken cancellationToken = default);
    Task RespondToP2PRequestAsync(RespondToP2PRequest request, CancellationToken cancellationToken = default);

    Task<SplitPaymentDto> CreateSplitPaymentAsync(SplitPaymentRequest request, CancellationToken cancellationToken = default);
    Task<SplitPaymentDto> PaySplitShareAsync(Guid splitId, Guid walletId, CancellationToken cancellationToken = default);

    Task<ScheduledWalletPaymentDto> CreateScheduledPaymentAsync(CreateScheduledPaymentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduledWalletPaymentDto>> GetScheduledPaymentsAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task CancelScheduledPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<LoyaltyPointsDto> GetLoyaltyPointsAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoyaltyTransactionDto>> GetLoyaltyTransactionsAsync(Guid walletId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<RedeemResultDto> RedeemPointsAsync(RedeemPointsRequest request, CancellationToken cancellationToken = default);

    Task<NFCTokenDto> GenerateNFCTokenAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task RevokeNFCTokenAsync(string token, CancellationToken cancellationToken = default);
}

public record TransferResultDto
{
    public Guid TransactionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public long NewBalanceMinorUnits { get; init; }
}

public record RedeemResultDto
{
    public long PointsRedeemed { get; init; }
    public long AmountCreditedMinorUnits { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
}
