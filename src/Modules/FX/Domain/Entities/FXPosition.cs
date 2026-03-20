namespace Finitech.Modules.FX.Domain.Entities;

/// <summary>
/// FX position tracking for risk management.
/// Merged from Bankin.
/// </summary>
public class FXPosition
{
    public Guid Id { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal LongAmount { get; set; }
    public decimal ShortAmount { get; set; }
    public decimal NetPosition => LongAmount - ShortAmount;
    public decimal DailyVolume { get; set; }
    public decimal MaxDailyLimit { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public bool CanTrade(decimal amount) => (DailyVolume + amount) <= MaxDailyLimit;
}
