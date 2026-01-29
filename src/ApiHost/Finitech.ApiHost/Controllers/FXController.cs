using Finitech.Modules.FX.Contracts;
using Finitech.Modules.FX.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Finitech.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FXController : ControllerBase
{
    private readonly IFXService _service;

    public FXController(IFXService service)
    {
        _service = service;
    }

    [HttpPost("rate")]
    public async Task<ActionResult<FXRateDto>> GetRate(FXRateRequest request)
    {
        var rate = await _service.GetRateAsync(request);
        return Ok(rate);
    }

    [HttpPost("quote")]
    public async Task<ActionResult<FXQuoteResponse>> GetQuote(FXQuoteRequest request)
    {
        var quote = await _service.GetQuoteAsync(request);
        return Ok(quote);
    }

    [HttpPost("convert")]
    public async Task<ActionResult<FXConvertResponse>> Convert(FXConvertRequest request)
    {
        var result = await _service.ConvertAsync(request);
        return Ok(result);
    }

    [HttpGet("rates/{fromCurrency}/{toCurrency}")]
    public async Task<ActionResult<FXRateDto>> GetRate(string fromCurrency, string toCurrency)
    {
        var rate = await _service.GetRateAsync(new FXRateRequest
        {
            FromCurrencyCode = fromCurrency,
            ToCurrencyCode = toCurrency
        });
        return Ok(rate);
    }
}
