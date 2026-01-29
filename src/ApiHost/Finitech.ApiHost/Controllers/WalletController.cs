using Finitech.Modules.Wallet.Contracts;
using Finitech.Modules.Wallet.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Finitech.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _service;

    public WalletController(IWalletService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<WalletAccountDto>> CreateWallet(CreateWalletRequest request)
    {
        var wallet = await _service.CreateWalletAsync(request);
        return CreatedAtAction(nameof(GetWallet), new { walletId = wallet.Id }, wallet);
    }

    [HttpGet("{walletId:guid}")]
    public async Task<ActionResult<WalletAccountDto>> GetWallet(Guid walletId)
    {
        var wallet = await _service.GetWalletAsync(walletId);
        return wallet == null ? NotFound() : Ok(wallet);
    }

    [HttpGet("by-party/{partyId:guid}")]
    public async Task<ActionResult<WalletAccountDto>> GetWalletByPartyId(Guid partyId)
    {
        var wallet = await _service.GetWalletByPartyIdAsync(partyId);
        return wallet == null ? NotFound() : Ok(wallet);
    }

    [HttpGet("{walletId:guid}/limits")]
    public async Task<ActionResult<IReadOnlyList<WalletLimitsDto>>> GetWalletLimits(Guid walletId)
    {
        var limits = await _service.GetWalletLimitsAsync(walletId);
        return Ok(limits);
    }

    [HttpPost("p2p/send")]
    public async Task<ActionResult<TransferResultDto>> P2PSend(P2PSendRequest request)
    {
        var result = await _service.P2PSendAsync(request);
        return Ok(result);
    }

    [HttpPost("p2p/request")]
    public async Task<ActionResult<P2PRequestDto>> P2PRequestMoney(P2PRequestMoneyRequest request)
    {
        var result = await _service.P2PRequestMoneyAsync(request);
        return Ok(result);
    }

    [HttpPost("p2p/respond")]
    public async Task<IActionResult> RespondToP2PRequest(RespondToP2PRequest request)
    {
        await _service.RespondToP2PRequestAsync(request);
        return NoContent();
    }

    [HttpPost("split")]
    public async Task<ActionResult<SplitPaymentDto>> CreateSplitPayment(SplitPaymentRequest request)
    {
        var result = await _service.CreateSplitPaymentAsync(request);
        return Ok(result);
    }

    [HttpPost("split/{splitId:guid}/pay/{walletId:guid}")]
    public async Task<ActionResult<SplitPaymentDto>> PaySplitShare(Guid splitId, Guid walletId)
    {
        var result = await _service.PaySplitShareAsync(splitId, walletId);
        return Ok(result);
    }

    [HttpPost("scheduled")]
    public async Task<ActionResult<ScheduledWalletPaymentDto>> CreateScheduledPayment(CreateScheduledPaymentRequest request)
    {
        var result = await _service.CreateScheduledPaymentAsync(request);
        return Ok(result);
    }

    [HttpGet("{walletId:guid}/loyalty")]
    public async Task<ActionResult<LoyaltyPointsDto>> GetLoyaltyPoints(Guid walletId)
    {
        var result = await _service.GetLoyaltyPointsAsync(walletId);
        return Ok(result);
    }

    [HttpPost("{walletId:guid}/loyalty/redeem")]
    public async Task<ActionResult<RedeemResultDto>> RedeemPoints(Guid walletId, [FromBody] long points)
    {
        var result = await _service.RedeemPointsAsync(new RedeemPointsRequest
        {
            WalletId = walletId,
            Points = points,
            RedemptionType = "Cashback"
        });
        return Ok(result);
    }

    [HttpPost("{walletId:guid}/nfc-token")]
    public async Task<ActionResult<NFCTokenDto>> GenerateNFCToken(Guid walletId)
    {
        var result = await _service.GenerateNFCTokenAsync(walletId);
        return Ok(result);
    }
}
