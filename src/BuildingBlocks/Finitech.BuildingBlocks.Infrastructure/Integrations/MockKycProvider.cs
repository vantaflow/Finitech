using Finitech.BuildingBlocks.Domain.Integrations;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Integrations;

public class MockKycProvider : IKycProvider
{
    private readonly ILogger<MockKycProvider> _logger;

    public MockKycProvider(ILogger<MockKycProvider> logger)
    {
        _logger = logger;
    }

    public Task<KycSubmissionResult> SubmitKycAsync(KycRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK KYC] Submitting KYC for {FirstName} {LastName}", request.FirstName, request.LastName);

        return Task.FromResult(new KycSubmissionResult
        {
            ProviderReference = $"KYC-{Guid.NewGuid():N}",
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow
        });
    }

    public Task<KycStatusResult> GetStatusAsync(string providerReference, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK KYC] Getting status for {Reference}", providerReference);

        return Task.FromResult(new KycStatusResult
        {
            ProviderReference = providerReference,
            Status = "Approved",
            VerifiedAt = DateTime.UtcNow.AddMinutes(-5)
        });
    }
}
