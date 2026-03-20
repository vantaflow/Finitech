namespace Finitech.Modules.Wallet.Application.Services;

public interface IWalletRepository
{
    Task<WalletAccountDto?> GetByIdAsync(Guid walletId);
    Task<List<WalletAccountDto>> GetByPartyAsync(Guid partyId);
    Task<WalletAccountDto> CreateAsync(Guid partyId, string currencyCode);
    Task<bool> TransferAsync(Guid fromId, Guid toId, long amountMinorUnits, string idempotencyKey);
}

public class WalletApplicationService
{
    private readonly IWalletRepository _repo;

    public WalletApplicationService(IWalletRepository repo) => _repo = repo;

    public Task<WalletAccountDto> CreateWalletAsync(Guid partyId, string currencyCode)
        => _repo.CreateAsync(partyId, currencyCode);

    public async Task<TransferResult> TransferP2PAsync(Guid fromWalletId, Guid toWalletId, long amountMinorUnits, string idempotencyKey)
    {
        if (amountMinorUnits <= 0)
            return new TransferResult(false, "Amount must be positive");

        var success = await _repo.TransferAsync(fromWalletId, toWalletId, amountMinorUnits, idempotencyKey);
        return new TransferResult(success, success ? "Completed" : "Failed");
    }

    public Task<WalletAccountDto?> GetWalletAsync(Guid walletId) => _repo.GetByIdAsync(walletId);
    public Task<List<WalletAccountDto>> GetWalletsByPartyAsync(Guid partyId) => _repo.GetByPartyAsync(partyId);
}

public record WalletAccountDto(Guid Id, Guid PartyId, string CurrencyCode, long BalanceMinorUnits, string Status);
public record TransferResult(bool Success, string Message);
