using Finitech.Modules.Ledger.Contracts;
using Finitech.Modules.Ledger.Contracts.DTOs;
using Finitech.Modules.Ledger.Domain;
using Finitech.Modules.Ledger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Ledger.Infrastructure.Services;

public class LedgerService : ILedgerService
{
    private readonly LedgerEntryRepository _entryRepository;
    private readonly AccountBalanceRepository _balanceRepository;
    private readonly BuildingBlocks.Domain.Repositories.IUnitOfWork _unitOfWork;

    public LedgerService(
        LedgerEntryRepository entryRepository,
        AccountBalanceRepository balanceRepository,
        BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork)
    {
        _entryRepository = entryRepository;
        _balanceRepository = balanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GetBalancesResponse> GetBalancesAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var balances = await _balanceRepository.GetByAccountAsync(accountId, cancellationToken);

        return new GetBalancesResponse
        {
            AccountId = accountId,
            Balances = balances.Select(b => new BalanceDto
            {
                CurrencyCode = b.CurrencyCode,
                AmountMinorUnits = b.BalanceMinorUnits,
                AmountDecimal = b.BalanceMinorUnits / 100m // Simplified conversion
            }).ToList()
        };
    }

    public async Task<BalanceDto?> GetBalanceAsync(Guid accountId, string currencyCode, CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetByAccountAndCurrencyAsync(accountId, currencyCode, cancellationToken);

        if (balance == null) return null;

        return new BalanceDto
        {
            CurrencyCode = balance.CurrencyCode,
            AmountMinorUnits = balance.BalanceMinorUnits,
            AmountDecimal = balance.BalanceMinorUnits / 100m
        };
    }

    public async Task<GetHistoryResponse> GetHistoryAsync(Guid accountId, GetHistoryRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<LedgerEntry> query = _entryRepository.AsQueryable()
            .Where(e => e.AccountId == accountId);

        if (!string.IsNullOrEmpty(request.CurrencyCode))
        {
            query = query.Where(e => e.CurrencyCode == request.CurrencyCode);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(e => e.EntryDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(e => e.EntryDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(request.TransactionType))
        {
            query = query.Where(e => e.EntryType.ToString() == request.TransactionType);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(e => e.EntryDate)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new GetHistoryResponse
        {
            AccountId = accountId,
            CurrencyCode = request.CurrencyCode ?? "",
            Entries = entries.Select(e => new LedgerEntryDto
            {
                Id = e.Id,
                AccountId = e.AccountId,
                EntryType = e.EntryType.ToString(),
                CurrencyCode = e.CurrencyCode,
                AmountMinorUnits = e.AmountMinorUnits,
                AmountDecimal = e.AmountMinorUnits / 100m,
                Description = e.Description,
                Reference = e.Reference,
                TransactionId = e.TransactionId,
                OriginalTransactionId = e.OriginalEntryId,
                EntryDate = e.EntryDate,
                RunningBalance = e.RunningBalance
            }).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<PostTransactionResponse> PostTransactionAsync(PostTransactionRequest request, CancellationToken cancellationToken = default)
    {
        // Check idempotency
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingEntry = await _entryRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
            if (existingEntry != null)
            {
                return new PostTransactionResponse
                {
                    TransactionId = existingEntry.Id,
                    Status = "AlreadyProcessed",
                    NewBalanceMinorUnits = existingEntry.RunningBalance
                };
            }
        }

        // Get or create balance
        var balance = await _balanceRepository.GetByAccountAndCurrencyAsync(request.AccountId, request.CurrencyCode, cancellationToken);
        if (balance == null)
        {
            balance = AccountBalance.Create(request.AccountId, request.CurrencyCode);
            await _balanceRepository.AddAsync(balance, cancellationToken);
        }

        // Determine entry type
        var entryType = request.EntryType.Equals("Credit", StringComparison.OrdinalIgnoreCase)
            ? LedgerEntryType.Credit
            : LedgerEntryType.Debit;

        // Check sufficient balance for debits
        if (entryType == LedgerEntryType.Debit && !balance.HasSufficientBalance(request.AmountMinorUnits))
        {
            return new PostTransactionResponse
            {
                TransactionId = Guid.Empty,
                Status = "InsufficientBalance",
                NewBalanceMinorUnits = balance.BalanceMinorUnits
            };
        }

        // Update balance
        if (entryType == LedgerEntryType.Credit)
        {
            balance.Credit(request.AmountMinorUnits);
        }
        else
        {
            balance.Debit(request.AmountMinorUnits);
        }

        await _balanceRepository.UpdateAsync(balance, cancellationToken);

        // Create ledger entry
        var entry = LedgerEntry.Create(
            request.AccountId,
            request.CurrencyCode,
            entryType,
            request.AmountMinorUnits,
            request.Description,
            request.Reference,
            balance.BalanceMinorUnits,
            request.IdempotencyKey);

        await _entryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PostTransactionResponse
        {
            TransactionId = entry.Id,
            Status = "Posted",
            NewBalanceMinorUnits = balance.BalanceMinorUnits
        };
    }

    public async Task<PostTransactionResponse> VoidTransactionAsync(VoidTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var originalEntry = await _entryRepository.GetByIdAsync(request.OriginalTransactionId, cancellationToken);
        if (originalEntry == null)
        {
            return new PostTransactionResponse
            {
                TransactionId = Guid.Empty,
                Status = "OriginalTransactionNotFound",
                NewBalanceMinorUnits = 0
            };
        }

        var balance = await _balanceRepository.GetByAccountAndCurrencyAsync(originalEntry.AccountId, originalEntry.CurrencyCode, cancellationToken);
        if (balance == null)
        {
            return new PostTransactionResponse
            {
                TransactionId = Guid.Empty,
                Status = "BalanceNotFound",
                NewBalanceMinorUnits = 0
            };
        }

        // Reverse the transaction
        if (originalEntry.EntryType == LedgerEntryType.Credit)
        {
            balance.Debit(originalEntry.AmountMinorUnits);
        }
        else
        {
            balance.Credit(originalEntry.AmountMinorUnits);
        }

        await _balanceRepository.UpdateAsync(balance, cancellationToken);

        var voidEntry = LedgerEntry.CreateVoid(originalEntry, request.Reason, balance.BalanceMinorUnits);
        await _entryRepository.AddAsync(voidEntry, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PostTransactionResponse
        {
            TransactionId = voidEntry.Id,
            Status = "Voided",
            NewBalanceMinorUnits = balance.BalanceMinorUnits
        };
    }

    public async Task<PostTransactionResponse> ApplyAdjustmentAsync(AdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetByAccountAndCurrencyAsync(request.AccountId, request.CurrencyCode, cancellationToken);
        if (balance == null)
        {
            balance = AccountBalance.Create(request.AccountId, request.CurrencyCode);
            await _balanceRepository.AddAsync(balance, cancellationToken);
        }

        // Adjustment is always a credit (positive adjustment) or debit (negative)
        var entryType = request.AmountMinorUnits >= 0 ? LedgerEntryType.Credit : LedgerEntryType.Debit;
        var amount = Math.Abs(request.AmountMinorUnits);

        if (entryType == LedgerEntryType.Credit)
        {
            balance.Credit(amount);
        }
        else
        {
            balance.Debit(amount);
        }

        await _balanceRepository.UpdateAsync(balance, cancellationToken);

        var entry = LedgerEntry.Create(
            request.AccountId,
            request.CurrencyCode,
            entryType,
            amount,
            $"Adjustment ({request.AdjustmentType}): {request.Reason}",
            $"ADJ-{request.AdjustmentType}",
            balance.BalanceMinorUnits);

        await _entryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PostTransactionResponse
        {
            TransactionId = entry.Id,
            Status = "AdjustmentApplied",
            NewBalanceMinorUnits = balance.BalanceMinorUnits
        };
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid accountId, string currencyCode, long amountMinorUnits, CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetByAccountAndCurrencyAsync(accountId, currencyCode, cancellationToken);
        return balance?.HasSufficientBalance(amountMinorUnits) ?? false;
    }

    // Helper method for seeding data
    public async Task SeedBalance(Guid accountId, string currencyCode, long amountMinorUnits)
    {
        var balance = await _balanceRepository.GetByAccountAndCurrencyAsync(accountId, currencyCode);
        if (balance == null)
        {
            balance = AccountBalance.Create(accountId, currencyCode);
            await _balanceRepository.AddAsync(balance);
        }

        balance.Credit(amountMinorUnits);
        await _balanceRepository.UpdateAsync(balance);
        await _unitOfWork.SaveChangesAsync();
    }
}
