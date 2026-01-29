namespace Finitech.Modules.PartyRegistry.Contracts.Events;

/// <summary>
/// Event raised when a new party is created.
/// </summary>
public record PartyCreatedEvent
{
    public Guid PartyId { get; init; }
    public string PartyType { get; init; } = string.Empty; // Individual, Business
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Event raised when a role is assigned to a party.
/// </summary>
public record PartyRoleAssignedEvent
{
    public Guid PartyId { get; init; }
    public string Role { get; init; } = string.Empty; // Consumer, Merchant, RetailAgent, Distributor, Institution, RetailCustomer, ProCustomer
    public string Domain { get; init; } = string.Empty; // Wallet, Banking
    public DateTime AssignedAt { get; init; }
}

/// <summary>
/// Event raised when a party status changes.
/// </summary>
public record PartyStatusChangedEvent
{
    public Guid PartyId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public DateTime ChangedAt { get; init; }
}
