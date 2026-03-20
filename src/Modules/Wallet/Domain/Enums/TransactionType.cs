namespace Finitech.Modules.Wallet.Domain.Enums;

/// <summary>
/// Types de transactions supportées par le module Wallet.
/// </summary>
public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    BillPayment,
    Fee,
    Cashback,
    Bonus
}
