namespace Finitech.Modules.FX.Domain.Entities;

/// <summary>
/// FX deal representing a currency conversion transaction.
/// Supports T+2 settlement, commission, idempotency.
/// Merged from Bankin.
/// </summary>
public class FXDeal
{
    public Guid Id { get; set; }
    public string DealNumber { get; set; } = string.Empty;
    public Guid QuoteId { get; set; }
    public string DealType { get; set; } = "Spot"; // Spot, Forward, Swap
    public string BuyCurrency { get; set; } = string.Empty;
    public string SellCurrency { get; set; } = string.Empty;
    public decimal BuyAmount { get; set; }
    public decimal SellAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime TradeDate { get; set; } = DateTime.UtcNow;
    public DateTime ValueDate { get; set; } // T+2 settlement
    public string CustomerId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public decimal Commission { get; set; }
    public string CommissionCurrency { get; set; } = "MAD";
    public string Status { get; set; } = "Pending"; // Pending, Executed, Settled, Cancelled, Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
