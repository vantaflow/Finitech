using Finitech.Modules.MerchantPayments.Contracts.DTOs;

namespace Finitech.Modules.MerchantPayments.Contracts;

public interface IMerchantPaymentsService
{
    Task<MerchantDto> CreateMerchantAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default);
    Task<MerchantDto?> GetMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<MerchantDto?> GetMerchantByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task SuspendMerchantAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default);
    Task ActivateMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);

    Task<DynamicQRDto> GenerateDynamicQRAsync(GenerateDynamicQRRequest request, CancellationToken cancellationToken = default);
    Task<ParsedQRDto> ParseQRAsync(string payload, CancellationToken cancellationToken = default);
    Task<PayByQRResponse> PayByQRAsync(PayByQRRequest request, CancellationToken cancellationToken = default);
}
