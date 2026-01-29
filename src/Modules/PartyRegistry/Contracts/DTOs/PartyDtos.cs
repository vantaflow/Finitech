namespace Finitech.Modules.PartyRegistry.Contracts.DTOs;

public record CreatePartyRequest
{
    public string PartyType { get; init; } = string.Empty; // Individual, Business
    public string DisplayName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? BusinessName { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? TaxId { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public AddressDto? Address { get; init; }
    public List<string> InitialRoles { get; init; } = new();
}

public record AddressDto
{
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
}

public record PartyDto
{
    public Guid Id { get; init; }
    public string PartyType { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public AddressDto? Address { get; init; }
    public string Status { get; init; } = string.Empty; // Active, Suspended, Closed
    public DateTime CreatedAt { get; init; }
    public List<PartyRoleDto> Roles { get; init; } = new();
}

public record PartyRoleDto
{
    public string Role { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record AssignRoleRequest
{
    public string Role { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty; // Wallet, Banking
}

public record UpdatePartyRequest
{
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public AddressDto? Address { get; init; }
}
