namespace Finitech.Modules.Banking.Contracts.DTOs;

public record BankAccountDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty; // Current, Savings, FixedDeposit
    public string Status { get; init; } = string.Empty; // Active, Suspended, Closed
    public string CurrencyCode { get; init; } = string.Empty;
    public long BalanceMinorUnits { get; init; }
    public decimal BalanceDecimal { get; init; }
    public DateTime OpenedAt { get; init; }
}

public record CreateBankAccountRequest
{
    public Guid PartyId { get; init; }
    public string AccountType { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = "MAD";
    public string? AccountNumber { get; init; }
}

public record SavingsAccountDto : BankAccountDto
{
    public decimal InterestRate { get; init; }
    public long MinimumBalanceMinorUnits { get; init; }
    public long AccruedInterestMinorUnits { get; init; }
    public DateTime? LastInterestCalculationAt { get; init; }
}

public record CreateSavingsAccountRequest
{
    public Guid PartyId { get; init; }
    public string CurrencyCode { get; init; } = "MAD";
    public decimal InterestRate { get; init; }
    public long MinimumBalanceMinorUnits { get; init; }
    public long InitialDepositMinorUnits { get; init; }
}

public record DepositRequest
{
    public Guid AccountId { get; init; }
    public long AmountMinorUnits { get; init; }
    public string? Description { get; init; }
}

public record WithdrawRequest
{
    public Guid AccountId { get; init; }
    public long AmountMinorUnits { get; init; }
    public string? Description { get; init; }
}

public record FixedDepositDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public long PrincipalAmountMinorUnits { get; init; }
    public decimal InterestRate { get; init; }
    public int DurationMonths { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime MaturityDate { get; init; }
    public long MaturityAmountMinorUnits { get; init; }
    public long AccruedInterestMinorUnits { get; init; }
    public bool AutoRenewal { get; init; }
    public string Status { get; init; } = string.Empty; // Active, Matured, WithdrawnEarly
}

public record CreateFixedDepositRequest
{
    public Guid SourceAccountId { get; init; }
    public long PrincipalAmountMinorUnits { get; init; }
    public int DurationMonths { get; init; }
    public decimal InterestRate { get; init; }
    public bool AutoRenewal { get; init; }
}

public record LoanDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string LoanNumber { get; init; } = string.Empty;
    public long PrincipalAmountMinorUnits { get; init; }
    public decimal InterestRate { get; init; }
    public int DurationMonths { get; init; }
    public long MonthlyPaymentMinorUnits { get; init; }
    public long RemainingBalanceMinorUnits { get; init; }
    public string Status { get; init; } = string.Empty; // Pending, Active, PastDue, Defaulted, Closed
    public DateTime DisbursedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public int RemainingInstallments { get; init; }
}

public record LoanRequestDto
{
    public Guid PartyId { get; init; }
    public long RequestedAmountMinorUnits { get; init; }
    public int RequestedDurationMonths { get; init; }
    public string Purpose { get; init; } = string.Empty;
}

public record LoanApplicationRequest
{
    public Guid PartyId { get; init; }
    public long RequestedAmountMinorUnits { get; init; }
    public int RequestedDurationMonths { get; init; }
    public string Purpose { get; init; } = string.Empty;
    public string? EmploymentStatus { get; init; }
    public long? MonthlyIncomeMinorUnits { get; init; }
}

public record LoanApprovalRequest
{
    public Guid LoanId { get; init; }
    public bool Approved { get; init; }
    public decimal? ApprovedInterestRate { get; init; }
    public string? RejectionReason { get; init; }
    public string ApprovedBy { get; init; } = string.Empty;
}

public record LoanRepaymentRequest
{
    public Guid LoanId { get; init; }
    public Guid FromAccountId { get; init; }
    public long AmountMinorUnits { get; init; }
}

public record OverdraftDto
{
    public Guid AccountId { get; init; }
    public long ApprovedLimitMinorUnits { get; init; }
    public long CurrentOverdraftMinorUnits { get; init; }
    public decimal InterestRate { get; init; }
    public long AccruedFeesMinorUnits { get; init; }
    public bool IsActive { get; init; }
}

public record SetOverdraftRequest
{
    public Guid AccountId { get; init; }
    public long LimitMinorUnits { get; init; }
    public decimal InterestRate { get; init; }
}

public record ChequeBookDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string BookNumber { get; init; } = string.Empty;
    public int StartNumber { get; init; }
    public int EndNumber { get; init; }
    public int RemainingCheques { get; init; }
    public string Status { get; init; } = string.Empty; // Active, Exhausted, Cancelled
    public DateTime OrderedAt { get; init; }
}

public record OrderChequeBookRequest
{
    public Guid AccountId { get; init; }
    public int NumberOfCheques { get; init; } = 25;
}

public record ChequeDepositDto
{
    public Guid Id { get; init; }
    public Guid ToAccountId { get; init; }
    public string ChequeNumber { get; init; } = string.Empty;
    public string? DraweeBank { get; init; }
    public long AmountMinorUnits { get; init; }
    public string Status { get; init; } = string.Empty; // PendingClearance, Cleared, Rejected
    public DateTime DepositedAt { get; init; }
    public DateTime? ClearedAt { get; init; }
}

public record DepositChequeRequest
{
    public Guid ToAccountId { get; init; }
    public string ChequeNumber { get; init; } = string.Empty;
    public string? DraweeBank { get; init; }
    public long AmountMinorUnits { get; init; }
}

public record CardDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string CardNumberMasked { get; init; } = string.Empty;
    public string CardType { get; init; } = string.Empty; // Debit, Credit
    public string Status { get; init; } = string.Empty; // Active, Frozen, Blocked
    public DateTime ExpiryDate { get; init; }
    public long DailyWithdrawalLimitMinorUnits { get; init; }
    public long DailyPaymentLimitMinorUnits { get; init; }
    public long? EcommerceLimitMinorUnits { get; init; }
}

public record UpdateCardLimitsRequest
{
    public Guid CardId { get; init; }
    public long? DailyWithdrawalLimitMinorUnits { get; init; }
    public long? DailyPaymentLimitMinorUnits { get; init; }
    public long? EcommerceLimitMinorUnits { get; init; }
}
