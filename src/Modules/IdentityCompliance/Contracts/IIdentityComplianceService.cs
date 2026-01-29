using Finitech.Modules.IdentityCompliance.Contracts.DTOs;

namespace Finitech.Modules.IdentityCompliance.Contracts;

public interface IIdentityComplianceService
{
    Task<KYCDto> SubmitKYCAsync(SubmitKYCRequest request, CancellationToken cancellationToken = default);
    Task<KYCDto> ReviewKYCAsync(Guid kycId, ReviewKYCRequest request, CancellationToken cancellationToken = default);
    Task<KYBDto> SubmitKYBAsync(SubmitKYBRequest request, CancellationToken cancellationToken = default);
    Task<KYBDto> ReviewKYBAsync(Guid kybId, ReviewKYBRequest request, CancellationToken cancellationToken = default);
    Task<AMLScreeningResultDto> ScreenPartyAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<KYCDto?> GetKYCStatusAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<KYBDto?> GetKYBStatusAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<bool> IsKYCApprovedAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<bool> IsKYBApprovedAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FraudCaseDto>> GetFraudCasesAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<FraudCaseDto> ExecuteStrongActionAsync(StrongActionRequest request, CancellationToken cancellationToken = default);
}
