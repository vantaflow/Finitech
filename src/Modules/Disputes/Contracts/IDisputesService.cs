using Finitech.Modules.Disputes.Contracts.DTOs;

namespace Finitech.Modules.Disputes.Contracts;

public interface IDisputesService
{
    Task<RefundResponse> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default);
    Task<ChargebackDto> InitiateChargebackAsync(ChargebackRequest request, CancellationToken cancellationToken = default);
    Task<RepresentmentDto> SubmitRepresentmentAsync(RepresentmentRequest request, CancellationToken cancellationToken = default);
    Task<ChargebackDto> ResolveChargebackAsync(ResolveChargebackRequest request, CancellationToken cancellationToken = default);
    Task<ChargebackDto?> GetChargebackAsync(Guid chargebackId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChargebackDto>> GetChargebacksByTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
