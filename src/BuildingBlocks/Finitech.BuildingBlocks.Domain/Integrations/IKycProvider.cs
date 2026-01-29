namespace Finitech.BuildingBlocks.Domain.Integrations;

public interface IKycProvider
{
    Task<KycSubmissionResult> SubmitKycAsync(KycRequest request, CancellationToken cancellationToken = default);
    Task<KycStatusResult> GetStatusAsync(string providerReference, CancellationToken cancellationToken = default);
}

public class KycRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
}

public class KycSubmissionResult
{
    public string ProviderReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class KycStatusResult
{
    public string ProviderReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
}
