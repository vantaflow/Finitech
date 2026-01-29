using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.Modules.Banking.Domain.Entities;

/// <summary>
/// Loan entity
/// </summary>
public class Loan : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid PartyId { get; private set; }
    public string LoanNumber { get; private set; } = string.Empty;
    public long PrincipalAmountMinorUnits { get; private set; }
    public decimal InterestRate { get; private set; }
    public int DurationMonths { get; private set; }
    public long MonthlyPaymentMinorUnits { get; private set; }
    public long RemainingBalanceMinorUnits { get; private set; }
    public int RemainingInstallments { get; private set; }
    public LoanStatus Status { get; private set; } = LoanStatus.Pending;
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? DisbursedAt { get; private set; }
    public DateTime? FirstPaymentDate { get; private set; }
    public DateTime? MaturityDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Loan() { } // EF Core

    public static Loan Create(
        Guid partyId,
        string loanNumber,
        long principalAmount,
        decimal interestRate,
        int durationMonths)
    {
        var monthlyPayment = CalculateMonthlyPayment(principalAmount, interestRate, durationMonths);

        return new Loan
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            LoanNumber = loanNumber,
            PrincipalAmountMinorUnits = principalAmount,
            InterestRate = interestRate,
            DurationMonths = durationMonths,
            MonthlyPaymentMinorUnits = monthlyPayment,
            RemainingBalanceMinorUnits = principalAmount,
            RemainingInstallments = durationMonths,
            Status = LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve(string approvedBy, decimal? approvedInterestRate = null)
    {
        if (Status != LoanStatus.Pending)
            throw new InvalidOperationException("Only pending loans can be approved");

        Status = LoanStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;

        if (approvedInterestRate.HasValue)
        {
            InterestRate = approvedInterestRate.Value;
            MonthlyPaymentMinorUnits = CalculateMonthlyPayment(
                PrincipalAmountMinorUnits, InterestRate, DurationMonths);
        }
    }

    public void Disburse()
    {
        if (Status != LoanStatus.Approved)
            throw new InvalidOperationException("Only approved loans can be disbursed");

        Status = LoanStatus.Active;
        DisbursedAt = DateTime.UtcNow;
        FirstPaymentDate = DateTime.UtcNow.AddMonths(1);
        MaturityDate = DateTime.UtcNow.AddMonths(DurationMonths);
    }

    public void Reject(string reason)
    {
        if (Status != LoanStatus.Pending)
            throw new InvalidOperationException("Only pending loans can be rejected");

        Status = LoanStatus.Rejected;
    }

    public void MakePayment(long amountMinorUnits)
    {
        if (Status != LoanStatus.Active)
            throw new InvalidOperationException("Loan is not active");

        if (amountMinorUnits <= 0)
            throw new ArgumentException("Payment amount must be positive");

        RemainingBalanceMinorUnits -= amountMinorUnits;
        RemainingInstallments--;

        if (RemainingBalanceMinorUnits <= 0)
        {
            RemainingBalanceMinorUnits = 0;
            RemainingInstallments = 0;
            Status = LoanStatus.Closed;
        }
    }

    private static long CalculateMonthlyPayment(long principal, decimal annualRate, int months)
    {
        if (annualRate == 0)
            return principal / months;

        var monthlyRate = annualRate / 12 / 100;
        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);
        var payment = principal * (monthlyRate * factor) / (factor - 1);

        return (long)Math.Ceiling(payment);
    }
}

public enum LoanStatus
{
    Pending,
    Approved,
    Rejected,
    Active,
    Closed,
    Defaulted
}
