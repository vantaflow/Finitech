namespace Finitech.Modules.PartyRegistry.Application.Services;

public interface IPartyRepository
{
    Task<PartyDto> CreateAsync(string partyType, string firstName, string lastName, string displayName, string email, string phoneNumber, List<string> roles);
    Task<PartyDto?> GetByIdAsync(Guid partyId);
    Task AssignRoleAsync(Guid partyId, string role, string domain);
}

public class PartyRegistryApplicationService
{
    private readonly IPartyRepository _repo;

    public PartyRegistryApplicationService(IPartyRepository repo) => _repo = repo;

    public Task<PartyDto> CreatePartyAsync(string partyType, string firstName, string lastName, string displayName, string email, string phoneNumber, List<string> initialRoles)
        => _repo.CreateAsync(partyType, firstName, lastName, displayName, email, phoneNumber, initialRoles);

    public Task AssignRoleAsync(Guid partyId, string role, string domain) => _repo.AssignRoleAsync(partyId, role, domain);
    public Task<PartyDto?> GetPartyAsync(Guid partyId) => _repo.GetByIdAsync(partyId);
}

public record PartyDto(Guid Id, string PartyType, string DisplayName, string Email, string PhoneNumber, List<string> Roles, string Status);
