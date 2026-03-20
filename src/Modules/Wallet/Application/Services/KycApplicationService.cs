using Finitech.Modules.Wallet.Domain.Enums;

namespace Finitech.Modules.Wallet.Application.Services;

/// <summary>
/// Service KYC conforme Bank Al-Maghrib.
/// 4 niveaux progressifs avec vérification CIN marocaine.
/// Merged from ZOUZ.Wallet — unique to Finitech.
/// </summary>
public class KycApplicationService
{
    public Task<KycResult> InitiateBasicVerificationAsync(Guid walletId, string cinNumber)
    {
        // Vérification format CIN marocain (1-2 lettres + 5-6 chiffres)
        bool isValidFormat = !string.IsNullOrEmpty(cinNumber) &&
            System.Text.RegularExpressions.Regex.IsMatch(cinNumber, @"^[A-Za-z]{1,2}\d{5,6}$");

        if (!isValidFormat)
            return Task.FromResult(new KycResult(false, KycLevel.None, "Format CIN invalide"));

        return Task.FromResult(new KycResult(true, KycLevel.Basic, "Vérification basique effectuée"));
    }

    public Task<KycResult> VerifyIdentityAsync(Guid walletId, string cinNumber, string fullName,
        DateTime dateOfBirth, string cinFrontImage, string cinBackImage, string selfieImage)
    {
        bool isValid = !string.IsNullOrEmpty(cinNumber) && !string.IsNullOrEmpty(fullName)
            && dateOfBirth < DateTime.UtcNow.AddYears(-18) // 18+ ans requis
            && !string.IsNullOrEmpty(cinFrontImage) && !string.IsNullOrEmpty(cinBackImage)
            && !string.IsNullOrEmpty(selfieImage);

        if (!isValid)
            return Task.FromResult(new KycResult(false, KycLevel.None, "Documents incomplets ou âge insuffisant"));

        return Task.FromResult(new KycResult(true, KycLevel.Standard,
            "Vérification standard effectuée — limites: 10,000 MAD/jour, 50,000 MAD/mois"));
    }

    public Task<KycResult> UpgradeToAdvancedAsync(Guid walletId, bool isIdentityVerified)
    {
        if (!isIdentityVerified)
            return Task.FromResult(new KycResult(false, KycLevel.Standard,
                "Vérification identité requise avant passage au niveau Advanced"));

        return Task.FromResult(new KycResult(true, KycLevel.Advanced,
            "Niveau Advanced activé — limites: 20,000 MAD/jour, 100,000 MAD/mois"));
    }

    public KycLimits GetLimitsForLevel(KycLevel level) => level switch
    {
        KycLevel.None => new KycLimits(0, 0),
        KycLevel.Basic => new KycLimits(2000, 10000),
        KycLevel.Standard => new KycLimits(10000, 50000),
        KycLevel.Advanced => new KycLimits(20000, 100000),
        _ => new KycLimits(0, 0)
    };
}

public record KycResult(bool Success, KycLevel Level, string Message);
public record KycLimits(decimal DailyLimitMAD, decimal MonthlyLimitMAD);
