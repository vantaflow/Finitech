using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.Modules.PartyRegistry.Domain;

public class Party : AggregateRoot
{
    private readonly List<PartyRole> _roles = new();

    public string PartyType { get; private set; } = string.Empty; // Individual, Business
    public string DisplayName { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? BusinessName { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? TaxId { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public PartyStatus Status { get; private set; } = PartyStatus.Active;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<PartyRole> Roles => _roles.AsReadOnly();

    private Party() { } // EF Core

    public static Party CreateIndividual(string firstName, string lastName, string? email, string? phoneNumber)
    {
        var party = new Party
        {
            Id = Guid.NewGuid(),
            PartyType = "Individual",
            DisplayName = $"{firstName} {lastName}",
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            Status = PartyStatus.Active
        };

        return party;
    }

    public static Party CreateBusiness(string businessName, string? registrationNumber, string? taxId, string? email, string? phoneNumber)
    {
        var party = new Party
        {
            Id = Guid.NewGuid(),
            PartyType = "Business",
            DisplayName = businessName,
            BusinessName = businessName,
            RegistrationNumber = registrationNumber,
            TaxId = taxId,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            Status = PartyStatus.Active
        };

        return party;
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContact(string? email, string? phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRole(string role, string domain)
    {
        if (_roles.Any(r => r.Role == role && r.Domain == domain && r.Status == RoleStatus.Active))
            throw new InvalidOperationException($"Role {role} in domain {domain} is already assigned");

        var partyRole = PartyRole.Create(role, domain);
        _roles.Add(partyRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = PartyStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = PartyStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasRole(string role) => _roles.Any(r => r.Role == role && r.Status == RoleStatus.Active);
    public bool HasRoleInDomain(string role, string domain) => _roles.Any(r => r.Role == role && r.Domain == domain && r.Status == RoleStatus.Active);
}

public enum PartyStatus
{
    Active,
    Suspended,
    Closed
}

public class PartyRole : Entity
{
    public string Role { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty; // Wallet, Banking
    public RoleStatus Status { get; private set; } = RoleStatus.Active;
    public DateTime AssignedAt { get; private set; }

    private PartyRole() { } // EF Core

    public static PartyRole Create(string role, string domain)
    {
        return new PartyRole
        {
            Id = Guid.NewGuid(),
            Role = role,
            Domain = domain,
            Status = RoleStatus.Active,
            AssignedAt = DateTime.UtcNow
        };
    }

    public void Revoke() => Status = RoleStatus.Revoked;
}

public enum RoleStatus
{
    Active,
    Revoked
}

public class Address : ValueObject
{
    public string? Street { get; }
    public string? City { get; }
    public string? PostalCode { get; }
    public string? Country { get; }

    public Address(string? street, string? city, string? postalCode, string? country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street ?? string.Empty;
        yield return City ?? string.Empty;
        yield return PostalCode ?? string.Empty;
        yield return Country ?? string.Empty;
    }
}
