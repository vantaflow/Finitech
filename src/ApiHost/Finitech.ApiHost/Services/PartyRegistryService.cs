using Finitech.Modules.PartyRegistry.Contracts;
using Finitech.Modules.PartyRegistry.Contracts.DTOs;
using System.Collections.Concurrent;

namespace Finitech.ApiHost.Services;

public class PartyRegistryService : IPartyRegistryService
{
    private readonly ConcurrentDictionary<Guid, PartyDto> _parties = new();
    private readonly ConcurrentDictionary<(Guid PartyId, string Role, string Domain), PartyRoleDto> _roles = new();

    public Task<PartyDto> CreatePartyAsync(CreatePartyRequest request, CancellationToken cancellationToken = default)
    {
        var party = new PartyDto
        {
            Id = Guid.NewGuid(),
            PartyType = request.PartyType,
            DisplayName = request.DisplayName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            Roles = new List<PartyRoleDto>()
        };

        foreach (var role in request.InitialRoles)
        {
            var partyRole = new PartyRoleDto
            {
                Role = role,
                Domain = role is "RetailCustomer" or "ProCustomer" ? "Banking" : "Wallet",
                AssignedAt = DateTime.UtcNow,
                Status = "Active"
            };
            party.Roles.Add(partyRole);
            _roles[(party.Id, partyRole.Role, partyRole.Domain)] = partyRole;
        }

        _parties[party.Id] = party;
        return Task.FromResult(party);
    }

    public Task<PartyDto?> GetPartyAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        _parties.TryGetValue(partyId, out var party);
        return Task.FromResult(party);
    }

    public Task<PartyDto> UpdatePartyAsync(Guid partyId, UpdatePartyRequest request, CancellationToken cancellationToken = default)
    {
        if (!_parties.TryGetValue(partyId, out var party))
            throw new InvalidOperationException($"Party {partyId} not found");

        var updatedParty = party with
        {
            DisplayName = request.DisplayName ?? party.DisplayName,
            Email = request.Email ?? party.Email,
            PhoneNumber = request.PhoneNumber ?? party.PhoneNumber,
            Address = request.Address ?? party.Address
        };

        _parties[partyId] = updatedParty;
        return Task.FromResult(updatedParty);
    }

    public Task AssignRoleAsync(Guid partyId, AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (!_parties.TryGetValue(partyId, out var party))
            throw new InvalidOperationException($"Party {partyId} not found");

        var role = new PartyRoleDto
        {
            Role = request.Role,
            Domain = request.Domain,
            AssignedAt = DateTime.UtcNow,
            Status = "Active"
        };

        var roles = party.Roles.ToList();
        roles.Add(role);
        _parties[partyId] = party with { Roles = roles };
        _roles[(partyId, request.Role, request.Domain)] = role;

        return Task.CompletedTask;
    }

    public Task SuspendPartyAsync(Guid partyId, string reason, CancellationToken cancellationToken = default)
    {
        if (!_parties.TryGetValue(partyId, out var party))
            throw new InvalidOperationException($"Party {partyId} not found");

        _parties[partyId] = party with { Status = "Suspended" };
        return Task.CompletedTask;
    }

    public Task ClosePartyAsync(Guid partyId, string reason, CancellationToken cancellationToken = default)
    {
        if (!_parties.TryGetValue(partyId, out var party))
            throw new InvalidOperationException($"Party {partyId} not found");

        _parties[partyId] = party with { Status = "Closed" };
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PartyDto>> ListByRoleAsync(string role, string domain, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var parties = _roles
            .Where(r => r.Value.Role == role && r.Value.Domain == domain && r.Value.Status == "Active")
            .Select(r => r.Key.PartyId)
            .Distinct()
            .Select(id => _parties.TryGetValue(id, out var p) ? p : null)
            .Where(p => p != null)
            .Cast<PartyDto>()
            .Skip(skip)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<PartyDto>>(parties);
    }

    public Task<bool> HasRoleAsync(Guid partyId, string role, CancellationToken cancellationToken = default)
    {
        if (!_parties.TryGetValue(partyId, out var party))
            return Task.FromResult(false);

        return Task.FromResult(party.Roles.Any(r => r.Role == role && r.Status == "Active"));
    }

    public Task<bool> IsActiveAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        if (!_parties.TryGetValue(partyId, out var party))
            return Task.FromResult(false);

        return Task.FromResult(party.Status == "Active");
    }
}
