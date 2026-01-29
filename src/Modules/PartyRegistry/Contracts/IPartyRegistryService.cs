using Finitech.Modules.PartyRegistry.Contracts.DTOs;

namespace Finitech.Modules.PartyRegistry.Contracts;

public interface IPartyRegistryService
{
    Task<PartyDto> CreatePartyAsync(CreatePartyRequest request, CancellationToken cancellationToken = default);
    Task<PartyDto?> GetPartyAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<PartyDto> UpdatePartyAsync(Guid partyId, UpdatePartyRequest request, CancellationToken cancellationToken = default);
    Task AssignRoleAsync(Guid partyId, AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task SuspendPartyAsync(Guid partyId, string reason, CancellationToken cancellationToken = default);
    Task ClosePartyAsync(Guid partyId, string reason, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartyDto>> ListByRoleAsync(string role, string domain, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid partyId, string role, CancellationToken cancellationToken = default);
    Task<bool> IsActiveAsync(Guid partyId, CancellationToken cancellationToken = default);
}
