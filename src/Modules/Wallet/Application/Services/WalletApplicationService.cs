using Finitech.Modules.Wallet.Infrastructure.Data;
using Finitech.Modules.Wallet.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Wallet.Application.Services;

public class WalletApplicationService
{
    private readonly WalletDbContext _db;

    public WalletApplicationService(WalletDbContext db)
    {
        _db = db;
    }

    public async Task<WalletAccount> CreateWalletAsync(Guid partyId, string currencyCode)
    {
        var wallet = new WalletAccount
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            CurrencyCode = currencyCode,
            BalanceMinorUnits = 0,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        _db.WalletAccounts.Add(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }

    public async Task<bool> TransferP2PAsync(Guid fromWalletId, Guid toWalletId, long amountMinorUnits, string idempotencyKey)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var sender = await _db.WalletAccounts.FindAsync(fromWalletId)
                ?? throw new InvalidOperationException("Sender wallet not found");
            var receiver = await _db.WalletAccounts.FindAsync(toWalletId)
                ?? throw new InvalidOperationException("Receiver wallet not found");

            if (sender.BalanceMinorUnits < amountMinorUnits)
                throw new InvalidOperationException("Insufficient balance");

            sender.BalanceMinorUnits -= amountMinorUnits;
            receiver.BalanceMinorUnits += amountMinorUnits;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<WalletAccount?> GetWalletAsync(Guid walletId)
        => await _db.WalletAccounts.FindAsync(walletId);

    public async Task<List<WalletAccount>> GetWalletsByPartyAsync(Guid partyId)
        => await _db.WalletAccounts.Where(w => w.PartyId == partyId).ToListAsync();
}
