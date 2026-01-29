using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.Modules.Banking.Domain.Entities;

/// <summary>
/// Payment card entity with tokenized PAN for PCI compliance
/// </summary>
public class Card : Entity
{
    public new Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string CardToken { get; private set; } = string.Empty; // Tokenized PAN
    public string CardNumberMasked { get; private set; } = string.Empty; // **** **** **** 1234
    public string CardType { get; private set; } = string.Empty; // Debit, Credit
    public string CardNetwork { get; private set; } = string.Empty; // Visa, Mastercard
    public DateTime ExpiryDate { get; private set; }
    public string EncryptedCvv { get; private set; } = string.Empty; // Encrypted CVV
    public string? EncryptedPin { get; private set; } // Encrypted PIN
    public CardStatus Status { get; private set; } = CardStatus.Active;
    public DateTime IssuedAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? BlockedAt { get; private set; }
    public string? BlockReason { get; private set; }

    // Limits
    public long DailyWithdrawalLimitMinorUnits { get; private set; }
    public long DailyPaymentLimitMinorUnits { get; private set; }
    public long MonthlyPaymentLimitMinorUnits { get; private set; }

    // Navigation
    public BankAccount Account { get; private set; } = null!;

    private Card() { } // EF Core

    public static Card Create(
        Guid accountId,
        string cardNumber,
        string cardType,
        string cardNetwork,
        DateTime expiryDate,
        string cvv,
        string? pin = null)
    {
        // Generate token and masked number
        var token = Guid.NewGuid().ToString("N");
        var masked = $"**** **** **** {cardNumber[^4..]}";

        return new Card
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CardToken = token,
            CardNumberMasked = masked,
            CardType = cardType,
            CardNetwork = cardNetwork,
            ExpiryDate = expiryDate,
            EncryptedCvv = cvv, // Should be encrypted before storage
            EncryptedPin = pin,
            Status = CardStatus.Inactive,
            IssuedAt = DateTime.UtcNow,
            DailyWithdrawalLimitMinorUnits = 500000, // 5,000 MAD default
            DailyPaymentLimitMinorUnits = 1000000,   // 10,000 MAD default
            MonthlyPaymentLimitMinorUnits = 10000000 // 100,000 MAD default
        };
    }

    public void Activate()
    {
        if (Status != CardStatus.Inactive)
            throw new InvalidOperationException("Card must be in inactive state to activate");

        Status = CardStatus.Active;
        ActivatedAt = DateTime.UtcNow;
    }

    public void Block(string reason)
    {
        Status = CardStatus.Blocked;
        BlockedAt = DateTime.UtcNow;
        BlockReason = reason;
    }

    public void Unblock()
    {
        Status = CardStatus.Active;
        BlockedAt = null;
        BlockReason = null;
    }

    public void UpdateLimits(long dailyWithdrawal, long dailyPayment, long monthlyPayment)
    {
        DailyWithdrawalLimitMinorUnits = dailyWithdrawal;
        DailyPaymentLimitMinorUnits = dailyPayment;
        MonthlyPaymentLimitMinorUnits = monthlyPayment;
    }
}

public enum CardStatus
{
    Inactive,
    Active,
    Blocked,
    Expired,
    Cancelled
}
