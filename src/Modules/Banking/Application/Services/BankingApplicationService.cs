namespace Finitech.Modules.Banking.Application.Services;

public interface IBankingRepository
{
    Task<BankAccountDto> OpenAccountAsync(Guid partyId, string accountType, string currencyCode);
    Task<BankAccountDto?> GetAccountAsync(Guid accountId);
    Task<List<BankAccountDto>> GetAccountsByPartyAsync(Guid partyId);
    Task<LoanDto> RequestLoanAsync(Guid partyId, long amountMinorUnits, int durationMonths, string purpose);
    Task<LoanDto> ApproveLoanAsync(Guid loanId, decimal interestRate, string approvedBy);
}

public class BankingApplicationService
{
    private readonly IBankingRepository _repo;

    public BankingApplicationService(IBankingRepository repo) => _repo = repo;

    public Task<BankAccountDto> OpenAccountAsync(Guid partyId, string accountType, string currencyCode)
        => _repo.OpenAccountAsync(partyId, accountType, currencyCode);

    public Task<LoanDto> RequestLoanAsync(Guid partyId, long amountMinorUnits, int durationMonths, string purpose)
        => _repo.RequestLoanAsync(partyId, amountMinorUnits, durationMonths, purpose);

    public Task<LoanDto> ApproveLoanAsync(Guid loanId, decimal interestRate, string approvedBy)
        => _repo.ApproveLoanAsync(loanId, interestRate, approvedBy);

    public Task<BankAccountDto?> GetAccountAsync(Guid accountId) => _repo.GetAccountAsync(accountId);
    public Task<List<BankAccountDto>> GetAccountsByPartyAsync(Guid partyId) => _repo.GetAccountsByPartyAsync(partyId);
}

public record BankAccountDto(Guid Id, Guid PartyId, string AccountType, string CurrencyCode, long BalanceMinorUnits, string Status);
public record LoanDto(Guid Id, Guid PartyId, long AmountMinorUnits, int DurationMonths, string Status, decimal? InterestRate);
