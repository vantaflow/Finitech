using Finitech.Modules.Wallet.Domain;

namespace Finitech.Modules.Wallet.Application.Services;

/// <summary>
/// Paiement de factures via wallet.
/// Supporte : Maroc Telecom, Inwi, Orange, REDAL, LYDEC, ONEE, impôts.
/// Merged from ZOUZ.Wallet — unique to Finitech.
/// </summary>
public class BillPaymentApplicationService
{
    private static readonly HashSet<string> SupportedBillers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Maroc Telecom", "Inwi", "Orange",          // Telecom
        "REDAL", "LYDEC", "ONEE",                     // Eau & Électricité
        "Direction Générale des Impôts"                // Taxes
    };

    private static readonly Dictionary<string, string> BillerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Maroc Telecom"] = "Telecom", ["Inwi"] = "Telecom", ["Orange"] = "Telecom",
        ["REDAL"] = "Eau", ["LYDEC"] = "Eau", ["ONEE"] = "Électricité",
        ["Direction Générale des Impôts"] = "Taxes"
    };

    public Task<BillVerificationResult> VerifyBillAsync(string billerName, string billerReference, string customerReference, decimal amount)
    {
        if (string.IsNullOrEmpty(billerName) || string.IsNullOrEmpty(billerReference) || string.IsNullOrEmpty(customerReference))
            return Task.FromResult(new BillVerificationResult(false, "Informations de facture incomplètes", null));

        if (!SupportedBillers.Contains(billerName))
            return Task.FromResult(new BillVerificationResult(false, $"Fournisseur non supporté: {billerName}", null));

        return Task.FromResult(new BillVerificationResult(true, "Facture vérifiée", DateTime.UtcNow.AddDays(15)));
    }

    public Task<BillPaymentResult> PayBillAsync(Guid walletId, string billerName, string billerReference,
        string customerReference, decimal amount)
    {
        if (!SupportedBillers.Contains(billerName))
            throw new ArgumentException($"Fournisseur non supporté: {billerName}");

        var billType = BillerTypes.GetValueOrDefault(billerName, "Autre");
        var transactionRef = Guid.NewGuid().ToString();

        return Task.FromResult(new BillPaymentResult(transactionRef, billType, "Completed", DateTime.UtcNow));
    }

    public IReadOnlyCollection<string> GetSupportedBillers() => SupportedBillers;
}

public record BillVerificationResult(bool IsValid, string Message, DateTime? DueDate);
public record BillPaymentResult(string TransactionReference, string BillType, string Status, DateTime Timestamp);
