using Finitech.Modules.PartyRegistry.Contracts;
using Finitech.Modules.PartyRegistry.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finitech.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartyRegistryController : ControllerBase
{
    private readonly IPartyRegistryService _service;

    public PartyRegistryController(IPartyRegistryService service)
    {
        _service = service;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<PartyDto>> CreateParty(CreatePartyRequest request)
    {
        var party = await _service.CreatePartyAsync(request);
        return CreatedAtAction(nameof(GetParty), new { partyId = party.Id }, party);
    }

    [HttpGet("{partyId:guid}")]
    public async Task<ActionResult<PartyDto>> GetParty(Guid partyId)
    {
        var party = await _service.GetPartyAsync(partyId);
        return party == null ? NotFound() : Ok(party);
    }

    [HttpPut("{partyId:guid}")]
    public async Task<ActionResult<PartyDto>> UpdateParty(Guid partyId, UpdatePartyRequest request)
    {
        var party = await _service.UpdatePartyAsync(partyId, request);
        return Ok(party);
    }

    [HttpPost("{partyId:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid partyId, AssignRoleRequest request)
    {
        await _service.AssignRoleAsync(partyId, request);
        return NoContent();
    }

    [HttpPost("{partyId:guid}/suspend")]
    public async Task<IActionResult> SuspendParty(Guid partyId, [FromBody] string reason)
    {
        await _service.SuspendPartyAsync(partyId, reason);
        return NoContent();
    }

    [HttpGet("by-role/{role}")]
    public async Task<ActionResult<IReadOnlyList<PartyDto>>> ListByRole(string role, [FromQuery] string domain = "Wallet", [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var parties = await _service.ListByRoleAsync(role, domain, skip, take);
        return Ok(parties);
    }
}
