using Finitech.Modules.Payments.Contracts.DTOs;

namespace Finitech.Modules.Payments.Contracts;

public interface IPaymentsService
{
    Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default);
    Task<TransferResponse> CrossCurrencyTransferAsync(CrossCurrencyTransferRequest request, CancellationToken cancellationToken = default);
    Task<TransferResponse> PayBillAsync(BillPayRequest request, CancellationToken cancellationToken = default);
    Task<TransferResponse> TopUpAsync(TopUpRequest request, CancellationToken cancellationToken = default);

    Task<BeneficiaryDto> CreateBeneficiaryAsync(CreateBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BeneficiaryDto>> GetBeneficiariesAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task DeleteBeneficiaryAsync(Guid beneficiaryId, CancellationToken cancellationToken = default);

    Task<StandingOrderDto> CreateStandingOrderAsync(CreateStandingOrderRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StandingOrderDto>> GetStandingOrdersAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task CancelStandingOrderAsync(Guid standingOrderId, CancellationToken cancellationToken = default);

    Task<DirectDebitMandateDto> CreateDirectDebitMandateAsync(CreateDirectDebitMandateRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DirectDebitMandateDto>> GetDirectDebitMandatesAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task CancelDirectDebitMandateAsync(Guid mandateId, CancellationToken cancellationToken = default);

    Task<ScheduledPaymentDto> SchedulePaymentAsync(SchedulePaymentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduledPaymentDto>> GetScheduledPaymentsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task CancelScheduledPaymentAsync(Guid scheduledPaymentId, CancellationToken cancellationToken = default);
}
