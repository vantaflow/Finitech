using Finitech.Modules.Wallet.Domain;

namespace Finitech.Modules.Wallet.Application.Services;

/// <summary>
/// Paiement de factures via wallet — fournisseurs marocains.
/// Architecture extensible via IBillProvider.
/// </summary>
public class BillPaymentApplicationService
{
    private readonly Dictionary<string, IBillProvider> _providers;

    public BillPaymentApplicationService()
    {
        _providers = new Dictionary<string, IBillProvider>(StringComparer.OrdinalIgnoreCase)
        {
            // Telecom
            ["Maroc Telecom"] = new TelecomBillProvider("Maroc Telecom", "api.iam.ma"),
            ["Inwi"] = new TelecomBillProvider("Inwi", "api.inwi.ma"),
            ["Orange"] = new TelecomBillProvider("Orange", "api.orange.ma"),
            // Eau & Électricité
            ["REDAL"] = new UtilityBillProvider("REDAL", "Eau", "api.redal.ma"),
            ["LYDEC"] = new UtilityBillProvider("LYDEC", "Eau", "api.lydec.ma"),
            ["ONEE Eau"] = new UtilityBillProvider("ONEE Eau", "Eau", "api.onee.ma/eau"),
            ["ONEE Électricité"] = new UtilityBillProvider("ONEE Électricité", "Électricité", "api.onee.ma/elec"),
            ["Amendis Tanger"] = new UtilityBillProvider("Amendis Tanger", "Eau", "api.amendis.ma/tanger"),
            ["Amendis Tétouan"] = new UtilityBillProvider("Amendis Tétouan", "Eau", "api.amendis.ma/tetouan"),
            // Taxes & Gouvernement
            ["Direction Générale des Impôts"] = new GovernmentBillProvider("DGI", "api.tax.gov.ma"),
            ["Trésorerie Générale du Royaume"] = new GovernmentBillProvider("TGR", "api.tgr.gov.ma"),
            // Internet & TV
            ["Maroc Telecom Fibre"] = new TelecomBillProvider("MT Fibre", "api.iam.ma/fibre"),
            ["Inwi Fibre"] = new TelecomBillProvider("Inwi Fibre", "api.inwi.ma/fibre"),
        };
    }

    public async Task<BillVerificationResult> VerifyBillAsync(string billerName, string billerRef, string customerRef, decimal amount)
    {
        if (!_providers.TryGetValue(billerName, out var provider))
            return new BillVerificationResult(false, $"Fournisseur non supporté: {billerName}. Supportés: {string.Join(", ", _providers.Keys)}", null, null);

        return await provider.VerifyAsync(billerRef, customerRef, amount);
    }

    public async Task<BillPaymentResult> PayBillAsync(Guid walletId, string billerName, string billerRef, string customerRef, decimal amount)
    {
        if (!_providers.TryGetValue(billerName, out var provider))
            throw new ArgumentException($"Fournisseur non supporté: {billerName}");

        var verification = await provider.VerifyAsync(billerRef, customerRef, amount);
        if (!verification.IsValid)
            throw new InvalidOperationException($"Vérification échouée: {verification.Message}");

        return await provider.PayAsync(walletId, billerRef, customerRef, amount);
    }

    public Dictionary<string, List<string>> GetSupportedBillersByCategory()
    {
        var result = new Dictionary<string, List<string>>();
        foreach (var (name, provider) in _providers)
        {
            var cat = provider.Category;
            if (!result.ContainsKey(cat)) result[cat] = new();
            result[cat].Add(name);
        }
        return result;
    }

    public IReadOnlyCollection<string> GetSupportedBillers() => _providers.Keys;
}

// === Provider interfaces and implementations ===

public interface IBillProvider
{
    string Name { get; }
    string Category { get; }
    Task<BillVerificationResult> VerifyAsync(string billerRef, string customerRef, decimal amount);
    Task<BillPaymentResult> PayAsync(Guid walletId, string billerRef, string customerRef, decimal amount);
}

public class TelecomBillProvider : IBillProvider
{
    public string Name { get; }
    public string Category => "Telecom";
    private readonly string _apiUrl;

    public TelecomBillProvider(string name, string apiUrl) { Name = name; _apiUrl = apiUrl; }

    public Task<BillVerificationResult> VerifyAsync(string billerRef, string customerRef, decimal amount)
    {
        if (string.IsNullOrEmpty(billerRef) || string.IsNullOrEmpty(customerRef))
            return Task.FromResult(new BillVerificationResult(false, "Référence incomplète", null, null));

        // In production: call _apiUrl to verify bill
        return Task.FromResult(new BillVerificationResult(true, "Facture vérifiée", DateTime.UtcNow.AddDays(15), amount));
    }

    public Task<BillPaymentResult> PayAsync(Guid walletId, string billerRef, string customerRef, decimal amount)
    {
        var txRef = $"TEL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";
        return Task.FromResult(new BillPaymentResult(txRef, Category, Name, "Completed", DateTime.UtcNow));
    }
}

public class UtilityBillProvider : IBillProvider
{
    public string Name { get; }
    public string Category { get; }
    private readonly string _apiUrl;

    public UtilityBillProvider(string name, string category, string apiUrl) { Name = name; Category = category; _apiUrl = apiUrl; }

    public Task<BillVerificationResult> VerifyAsync(string billerRef, string customerRef, decimal amount)
    {
        if (string.IsNullOrEmpty(billerRef) || string.IsNullOrEmpty(customerRef))
            return Task.FromResult(new BillVerificationResult(false, "Référence incomplète", null, null));

        return Task.FromResult(new BillVerificationResult(true, "Facture vérifiée", DateTime.UtcNow.AddDays(30), amount));
    }

    public Task<BillPaymentResult> PayAsync(Guid walletId, string billerRef, string customerRef, decimal amount)
    {
        var txRef = $"UTL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";
        return Task.FromResult(new BillPaymentResult(txRef, Category, Name, "Completed", DateTime.UtcNow));
    }
}

public class GovernmentBillProvider : IBillProvider
{
    public string Name { get; }
    public string Category => "Taxes & Gouvernement";
    private readonly string _apiUrl;

    public GovernmentBillProvider(string name, string apiUrl) { Name = name; _apiUrl = apiUrl; }

    public Task<BillVerificationResult> VerifyAsync(string billerRef, string customerRef, decimal amount)
    {
        if (string.IsNullOrEmpty(billerRef))
            return Task.FromResult(new BillVerificationResult(false, "Numéro de taxe requis", null, null));

        return Task.FromResult(new BillVerificationResult(true, "Taxe vérifiée", DateTime.UtcNow.AddDays(60), amount));
    }

    public Task<BillPaymentResult> PayAsync(Guid walletId, string billerRef, string customerRef, decimal amount)
    {
        var txRef = $"GOV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";
        return Task.FromResult(new BillPaymentResult(txRef, Category, Name, "Completed", DateTime.UtcNow));
    }
}

public record BillVerificationResult(bool IsValid, string Message, DateTime? DueDate, decimal? Amount);
public record BillPaymentResult(string TransactionReference, string Category, string BillerName, string Status, DateTime Timestamp);
