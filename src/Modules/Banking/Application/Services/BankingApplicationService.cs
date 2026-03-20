using Finitech.Modules.Banking.Infrastructure.Data;
using Finitech.Modules.Banking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Banking.Application.Services;

public class BankingApplicationService
{
    private readonly BankingDbContext _db;

    public BankingApplicationService(BankingDbContext db)
    {
        _db = db;
    }

    public async Task<BankAccount> OpenAccountAsync(Guid partyId, string accountType, string currencyCode)
    {
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            AccountType = accountType,
            CurrencyCode = currencyCode,
            BalanceMinorUnits = 0,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _db.BankAccounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task<Loan> RequestLoanAsync(Guid partyId, long amountMinorUnits, int durationMonths, string purpose)
    {
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            RequestedAmountMinorUnits = amountMinorUnits,
            DurationMonths = durationMonths,
            Purpose = purpose,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();
        return loan;
    }

    public async Task<Loan> ApproveLoanAsync(Guid loanId, decimal interestRate, string approvedBy)
    {
        var loan = await _db.Loans.FindAsync(loanId)
            ?? throw new InvalidOperationException("Loan not found");

        loan.Status = "Approved";
        loan.InterestRate = interestRate;
        loan.ApprovedBy = approvedBy;
        loan.ApprovedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return loan;
    }

    public async Task<BankAccount?> GetAccountAsync(Guid accountId)
    {
        return await _db.BankAccounts.FindAsync(accountId);
    }

    public async Task<List<BankAccount>> GetAccountsByPartyAsync(Guid partyId)
    {
        return await _db.BankAccounts
            .Where(a => a.PartyId == partyId)
            .ToListAsync();
    }
}
