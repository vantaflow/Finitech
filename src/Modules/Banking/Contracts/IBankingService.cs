using Finitech.Modules.Banking.Contracts.DTOs;

namespace Finitech.Modules.Banking.Contracts;

public interface IBankingService
{
    Task<BankAccountDto> CreateAccountAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default);
    Task<BankAccountDto?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankAccountDto>> GetAccountsByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task CloseAccountAsync(Guid accountId, string reason, CancellationToken cancellationToken = default);

    Task<SavingsAccountDto> CreateSavingsAccountAsync(CreateSavingsAccountRequest request, CancellationToken cancellationToken = default);
    Task DepositAsync(DepositRequest request, CancellationToken cancellationToken = default);
    Task WithdrawAsync(WithdrawRequest request, CancellationToken cancellationToken = default);
    Task CalculateInterestAsync(Guid savingsAccountId, CancellationToken cancellationToken = default);

    Task<FixedDepositDto> CreateFixedDepositAsync(CreateFixedDepositRequest request, CancellationToken cancellationToken = default);
    Task<FixedDepositDto?> GetFixedDepositAsync(Guid fixedDepositId, CancellationToken cancellationToken = default);
    Task WithdrawFixedDepositEarlyAsync(Guid fixedDepositId, CancellationToken cancellationToken = default);
    Task ProcessMaturedFixedDepositsAsync(CancellationToken cancellationToken = default);

    Task<LoanDto> RequestLoanAsync(LoanApplicationRequest request, CancellationToken cancellationToken = default);
    Task<LoanDto> ApproveLoanAsync(LoanApprovalRequest request, CancellationToken cancellationToken = default);
    Task DisburseLoanAsync(Guid loanId, CancellationToken cancellationToken = default);
    Task RepayLoanAsync(LoanRepaymentRequest request, CancellationToken cancellationToken = default);
    Task<LoanDto?> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanDto>> GetLoansByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default);

    Task<OverdraftDto> SetOverdraftAsync(SetOverdraftRequest request, CancellationToken cancellationToken = default);
    Task<OverdraftDto?> GetOverdraftAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task CalculateOverdraftFeesAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<ChequeBookDto> OrderChequeBookAsync(OrderChequeBookRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChequeBookDto>> GetChequeBooksAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task CancelChequeBookAsync(Guid chequeBookId, CancellationToken cancellationToken = default);
    Task<ChequeDepositDto> DepositChequeAsync(DepositChequeRequest request, CancellationToken cancellationToken = default);
    Task ProcessChequeClearanceAsync(Guid chequeDepositId, CancellationToken cancellationToken = default);
    Task RejectChequeAsync(Guid chequeDepositId, string reason, CancellationToken cancellationToken = default);

    Task<CardDto> GetCardAsync(Guid cardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardDto>> GetCardsByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task UpdateCardLimitsAsync(UpdateCardLimitsRequest request, CancellationToken cancellationToken = default);
    Task FreezeCardAsync(Guid cardId, CancellationToken cancellationToken = default);
    Task UnfreezeCardAsync(Guid cardId, CancellationToken cancellationToken = default);
    Task BlockCardAsync(Guid cardId, string reason, CancellationToken cancellationToken = default);
}
