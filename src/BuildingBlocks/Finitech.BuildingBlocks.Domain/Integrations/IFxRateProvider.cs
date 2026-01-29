namespace Finitech.BuildingBlocks.Domain.Integrations;

public interface IFxRateProvider
{
    Task<FxRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FxRate>> GetAllRatesAsync(CancellationToken cancellationToken = default);
}

public class FxRate
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal InverseRate { get; set; }
    public DateTime Timestamp { get; set; }
}
