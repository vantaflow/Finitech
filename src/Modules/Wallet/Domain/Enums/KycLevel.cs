namespace Finitech.Modules.Wallet.Domain.Enums;

/// <summary>
/// Niveaux KYC progressifs conformes aux exigences de Bank Al-Maghrib.
/// </summary>
public enum KycLevel
{
    None,       // Aucune vérification — compte limité
    Basic,      // Vérification téléphone + email
    Standard,   // Vérification CIN (Carte d'Identité Nationale)
    Advanced    // Vérification complète (CIN + justificatifs)
}
