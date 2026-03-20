namespace Finitech.Modules.FX.Domain.Entities;

/// <summary>
/// FX quote with bid/ask rates, spread and validity window.
/// Merged from Bankin — professional-grade FX trading.
/// </summary>
public class FXQuote
{
    public Guid Id { get; set; }
    public string QuoteId { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = "MAD";
    public string QuoteCurrency { get; set; } = "EUR";
    public decimal BidRate { get; set; }
    public decimal AskRate { get; set; }
    public decimal MidRate { get; set; }
    public decimal Spread { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime ValidTo { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired, Used, Cancelled
    public string Source { get; set; } = "ECB"; // External provider
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsValid() => Status == "Active" && DateTime.UtcNow <= ValidTo;
    public decimal GetRate(bool isBuy) => isBuy ? AskRate : BidRate;
}
