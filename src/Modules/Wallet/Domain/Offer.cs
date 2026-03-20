using Finitech.Modules.Wallet.Domain.Enums;

namespace Finitech.Modules.Wallet.Domain;

/// <summary>
/// Offre promotionnelle applicable aux wallets.
/// Supporte cashback, réduction de frais, bonus de recharge, fidélité et promotions spéciales.
/// </summary>
public class Offer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OfferType Type { get; set; }
    public decimal SpendingLimit { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public decimal? CashbackPercentage { get; set; }
    public decimal? FeesDiscount { get; set; }
    public decimal? RechargeBonus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
