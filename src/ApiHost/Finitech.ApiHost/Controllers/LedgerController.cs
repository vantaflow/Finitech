using Finitech.Modules.Ledger.Contracts;
using Finitech.Modules.Ledger.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Finitech.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly ILedgerService _service;

    public LedgerController(ILedgerService service)
    {
        _service = service;
    }

    [HttpGet("accounts/{accountId:guid}/balances")]
    public async Task<ActionResult<GetBalancesResponse>> GetBalances(Guid accountId)
    {
        var balances = await _service.GetBalancesAsync(accountId);
        return Ok(balances);
    }

    [HttpGet("accounts/{accountId:guid}/balances/{currencyCode}")]
    public async Task<ActionResult<BalanceDto>> GetBalance(Guid accountId, string currencyCode)
    {
        var balance = await _service.GetBalanceAsync(accountId, currencyCode);
        return balance == null ? NotFound() : Ok(balance);
    }

    [HttpPost("accounts/{accountId:guid}/history")]
    public async Task<ActionResult<GetHistoryResponse>> GetHistory(Guid accountId, GetHistoryRequest request)
    {
        var history = await _service.GetHistoryAsync(accountId, request);
        return Ok(history);
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<PostTransactionResponse>> PostTransaction(PostTransactionRequest request)
    {
        var result = await _service.PostTransactionAsync(request);
        return Ok(result);
    }

    [HttpPost("transactions/void")]
    public async Task<ActionResult<PostTransactionResponse>> VoidTransaction(VoidTransactionRequest request)
    {
        var result = await _service.VoidTransactionAsync(request);
        return Ok(result);
    }

    [HttpPost("adjustments")]
    public async Task<ActionResult<PostTransactionResponse>> ApplyAdjustment(AdjustmentRequest request)
    {
        var result = await _service.ApplyAdjustmentAsync(request);
        return Ok(result);
    }
}
