using Finitech.BuildingBlocks.Infrastructure.Repositories;
using Finitech.Modules.Ledger.Domain;
using Finitech.Modules.Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Ledger.Infrastructure.Repositories;

public class LedgerEntryRepository : Repository<LedgerEntry, LedgerDbContext>
{
    public LedgerEntryRepository(LedgerDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.AccountId == accountId)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetByAccountAndCurrencyAsync(Guid accountId, string currencyCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.AccountId == accountId && e.CurrencyCode == currencyCode)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<LedgerEntry?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.IdempotencyKey == idempotencyKey, cancellationToken);
    }
}

public class AccountBalanceRepository : Repository<AccountBalance, LedgerDbContext>
{
    public AccountBalanceRepository(LedgerDbContext context) : base(context)
    {
    }

    public async Task<AccountBalance?> GetByAccountAndCurrencyAsync(Guid accountId, string currencyCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(b => b.AccountId == accountId && b.CurrencyCode == currencyCode, cancellationToken);
    }

    public async Task<IReadOnlyList<AccountBalance>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.AccountId == accountId)
            .ToListAsync(cancellationToken);
    }
}
