namespace Finitech.Modules.Wallet.Infrastructure.Entities;

public class ScheduledPayment
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string PaymentType { get; set; } = string.Empty; // P2P, BillPay, etc.
    public string CurrencyCode { get; set; } = "MAD";
    public long AmountMinorUnits { get; set; }
    public string Frequency { get; set; } = "Monthly"; // Daily, Weekly, Monthly
    public DateTime NextExecutionAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Paused, Completed, Failed
    public string? RecipientWalletId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WalletAccount Wallet { get; set; } = null!;
}
