namespace Finitech.Modules.Wallet.Domain.Enums;

/// <summary>
/// Méthodes de paiement supportées (conformes au marché marocain).
/// </summary>
public enum PaymentMethod
{
    BankTransfer,
    CreditCard,
    OrangeMoney,
    InwiMoney,
    Cash
}
