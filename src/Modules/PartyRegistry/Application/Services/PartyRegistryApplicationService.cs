using Finitech.Modules.PartyRegistry.Contracts;
using Finitech.Modules.PartyRegistry.Domain;

namespace Finitech.Modules.PartyRegistry.Application.Services;

public class PartyRegistryApplicationService
{
    public async Task<Party> CreatePartyAsync(
        string partyType, string firstName, string lastName,
        string displayName, string email, string phoneNumber,
        List<string> initialRoles)
    {
        var party = new Party
        {
            Id = Guid.NewGuid(),
            PartyType = partyType,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            Email = email,
            PhoneNumber = phoneNumber,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            Roles = initialRoles
        };

        // Persist via repository
        return await Task.FromResult(party);
    }

    public async Task AssignRoleAsync(Guid partyId, string role, string domain)
    {
        // Load party, add role, save
        await Task.CompletedTask;
    }

    public async Task<Party?> GetPartyAsync(Guid partyId)
    {
        return await Task.FromResult<Party?>(null);
    }
}
