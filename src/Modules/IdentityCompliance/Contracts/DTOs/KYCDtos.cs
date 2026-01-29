namespace Finitech.Modules.IdentityCompliance.Contracts.DTOs;

public record SubmitKYCRequest
{
    public Guid PartyId { get; init; }
    public string DocumentType { get; init; } = string.Empty; // NationalId, Passport, ResidencePermit
    public string DocumentNumber { get; init; } = string.Empty;
    public DateTime DocumentExpiryDate { get; init; }
    public string? DocumentFrontImageUrl { get; init; }
    public string? DocumentBackImageUrl { get; init; }
    public string? SelfieImageUrl { get; init; }
    public AddressDto? Address { get; init; }
    public string? ProofOfAddressDocumentUrl { get; init; }
}

public record AddressDto
{
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
}

public record SubmitKYBRequest
{
    public Guid PartyId { get; init; }
    public string BusinessType { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public string? TaxId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string? RegistrationDocumentUrl { get; init; }
    public string? ArticlesOfAssociationUrl { get; init; }
    public List<BeneficialOwnerDto> BeneficialOwners { get; init; } = new();
}

public record BeneficialOwnerDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public decimal OwnershipPercentage { get; init; }
}

public record KYCDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // Draft, Submitted, InReview, Approved, Rejected
    public string? RejectionReason { get; init; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ReviewedBy { get; init; }
}

public record KYBDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string BusinessType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
}

public record ReviewKYCRequest
{
    public string Decision { get; init; } = string.Empty; // Approved, Rejected, EnhancedDueDiligence
    public string? RejectionReason { get; init; }
    public string ReviewedBy { get; init; } = string.Empty;
}

public record ReviewKYBRequest
{
    public string Decision { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public string ReviewedBy { get; init; } = string.Empty;
}

public record AMLScreeningResultDto
{
    public Guid PartyId { get; init; }
    public string RiskLevel { get; init; } = string.Empty; // Low, Medium, High
    public int RiskScore { get; init; }
    public List<AMLScreeningHitDto> Hits { get; init; } = new();
}

public record AMLScreeningHitDto
{
    public string ListName { get; init; } = string.Empty;
    public string MatchType { get; init; } = string.Empty;
    public string? Reason { get; init; }
}

public record FraudCaseDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string CaseType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // Open, UnderReview, TruePositive, FalsePositive, Closed
    public string RiskLevel { get; init; } = string.Empty; // Low, Medium, High, Critical
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? AssignedTo { get; init; }
}

public record StrongActionRequest
{
    public string ActionType { get; init; } = string.Empty; // FreezeParty, MarkSuspicious, OrderAccountClosure
    public Guid PartyId { get; init; }
    public string? AccountId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string InitiatedBy { get; init; } = string.Empty;
}

public record StrongActionDto
{
    public Guid Id { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // Pending, Applied, Reverted
    public string Reason { get; init; } = string.Empty;
    public DateTime InitiatedAt { get; init; }
    public string InitiatedBy { get; init; } = string.Empty;
}
