using Finitech.Modules.FX.Contracts.DTOs;

namespace Finitech.Modules.FX.Contracts;

public interface IFXService
{
    Task<FXRateDto> GetRateAsync(FXRateRequest request, CancellationToken cancellationToken = default);
    Task<FXQuoteResponse> GetQuoteAsync(FXQuoteRequest request, CancellationToken cancellationToken = default);
    Task<FXConvertResponse> ConvertAsync(FXConvertRequest request, CancellationToken cancellationToken = default);
}
