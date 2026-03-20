using Finitech.Modules.Wallet.Domain.Enums;

namespace Finitech.Modules.Wallet.Application.Services;

/// <summary>
/// Détection de fraude pour les transactions wallet.
/// Règles basées sur les seuils Bank Al-Maghrib.
/// Merged from ZOUZ.Wallet — unique to Finitech.
/// </summary>
public class FraudDetectionService
{
    public Task<FraudCheckResult> CheckDepositAsync(Guid walletId, decimal amount, PaymentMethod method)
    {
        var flags = new List<string>();

        if (amount > 10000) flags.Add("Montant élevé (>10,000 MAD)");
        if (method == PaymentMethod.Cash && amount > 5000) flags.Add("Dépôt cash élevé (>5,000 MAD)");

        return Task.FromResult(new FraudCheckResult(flags.Count == 0, flags));
    }

    public Task<FraudCheckResult> CheckWithdrawalAsync(Guid walletId, decimal amount, PaymentMethod method)
    {
        var flags = new List<string>();

        if (amount > 5000) flags.Add("Retrait élevé (>5,000 MAD)");

        return Task.FromResult(new FraudCheckResult(flags.Count == 0, flags));
    }

    public Task<FraudCheckResult> CheckTransferAsync(Guid sourceWalletId, Guid destWalletId, decimal amount)
    {
        var flags = new List<string>();

        if (amount > 7000) flags.Add("Transfert élevé (>7,000 MAD)");
        if (sourceWalletId == destWalletId) flags.Add("Transfert vers soi-même");

        return Task.FromResult(new FraudCheckResult(flags.Count == 0, flags));
    }
}

public record FraudCheckResult(bool IsClean, List<string> Flags);
