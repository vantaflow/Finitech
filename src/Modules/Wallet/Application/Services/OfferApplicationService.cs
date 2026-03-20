using Finitech.Modules.Wallet.Domain;
using Finitech.Modules.Wallet.Domain.Enums;

namespace Finitech.Modules.Wallet.Application.Services;

/// <summary>
/// Gestion des offres promotionnelles pour les wallets.
/// 5 types : Cashback, ReducedFees, RechargeBonus, LoyaltyProgram, SpecialPromotion.
/// Merged from ZOUZ.Wallet — unique to Finitech.
/// </summary>
public class OfferApplicationService
{
    public Task<Offer> CreateOfferAsync(string name, string description, OfferType type,
        decimal spendingLimit, DateTime validFrom, DateTime validTo,
        decimal? cashbackPct = null, decimal? feeDiscount = null, decimal? rechargeBonus = null)
    {
        if (validFrom >= validTo)
            throw new ArgumentException("La date de début doit être antérieure à la date de fin");
        if (spendingLimit <= 0)
            throw new ArgumentException("La limite de dépense doit être positive");

        ValidateOfferTypeParams(type, cashbackPct, feeDiscount, rechargeBonus);

        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            SpendingLimit = spendingLimit,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = true,
            CashbackPercentage = cashbackPct,
            FeesDiscount = feeDiscount,
            RechargeBonus = rechargeBonus,
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(offer);
    }

    public Task<bool> ActivateOfferAsync(Offer offer)
    {
        if (offer.ValidTo < DateTime.UtcNow)
            throw new InvalidOperationException("Impossible d'activer une offre expirée");

        offer.IsActive = true;
        offer.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> DeactivateOfferAsync(Offer offer)
    {
        offer.IsActive = false;
        offer.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }

    private static void ValidateOfferTypeParams(OfferType type, decimal? cashback, decimal? fees, decimal? recharge)
    {
        switch (type)
        {
            case OfferType.Cashback when (!cashback.HasValue || cashback <= 0 || cashback > 100):
                throw new ArgumentException("Cashback doit être entre 0 et 100%");
            case OfferType.ReducedFees when (!fees.HasValue || fees <= 0 || fees > 100):
                throw new ArgumentException("Réduction frais doit être entre 0 et 100%");
            case OfferType.RechargeBonus when (!recharge.HasValue || recharge <= 0 || recharge > 100):
                throw new ArgumentException("Bonus recharge doit être entre 0 et 100%");
        }
    }
}
