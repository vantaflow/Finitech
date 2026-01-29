using Finitech.BuildingBlocks.SharedKernel.Money;
using Finitech.Modules.Ledger.Contracts;
using Finitech.Modules.Ledger.Contracts.DTOs;
using System.Collections.Concurrent;

namespace Finitech.ApiHost.Services;

public class LedgerService : ILedgerService
{
    private readonly ConcurrentDictionary<(Guid AccountId, string CurrencyCode), long> _balances = new();
    private readonly ConcurrentDictionary<Guid, List<LedgerEntryDto>> _entries = new();
    private readonly ConcurrentDictionary<string, PostTransactionResponse> _idempotencyKeys = new();

    public Task<GetBalancesResponse> GetBalancesAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var balances = _balances
            .Where(b => b.Key.AccountId == accountId)
            .Select(b => new BalanceDto
            {
                CurrencyCode = b.Key.CurrencyCode,
                AmountMinorUnits = b.Value,
                AmountDecimal = b.Value / 100m,
                CurrencyNumericCode = GetNumericCode(b.Key.CurrencyCode)
            })
            .ToList();

        return Task.FromResult(new GetBalancesResponse
        {
            AccountId = accountId,
            Balances = balances
        });
    }

    public Task<BalanceDto?> GetBalanceAsync(Guid accountId, string currencyCode, CancellationToken cancellationToken = default)
    {
        if (_balances.TryGetValue((accountId, currencyCode), out var balance))
        {
            return Task.FromResult<BalanceDto?>(new BalanceDto
            {
                CurrencyCode = currencyCode,
                AmountMinorUnits = balance,
                AmountDecimal = balance / 100m,
                CurrencyNumericCode = GetNumericCode(currencyCode)
            });
        }

        return Task.FromResult<BalanceDto?>(null);
    }

    public Task<GetHistoryResponse> GetHistoryAsync(Guid accountId, GetHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var entries = _entries.TryGetValue(accountId, out var accountEntries)
            ? accountEntries
                .Where(e => request.CurrencyCode == null || e.CurrencyCode == request.CurrencyCode)
                .Where(e => request.FromDate == null || e.EntryDate >= request.FromDate)
                .Where(e => request.ToDate == null || e.EntryDate <= request.ToDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToList()
            : new List<LedgerEntryDto>();

        return Task.FromResult(new GetHistoryResponse
        {
            AccountId = accountId,
            CurrencyCode = request.CurrencyCode ?? "ALL",
            Entries = entries,
            TotalCount = entries.Count
        });
    }

    public Task<PostTransactionResponse> PostTransactionAsync(PostTransactionRequest request, CancellationToken cancellationToken = default)
    {
        // Idempotency check
        if (request.IdempotencyKey != null && _idempotencyKeys.TryGetValue(request.IdempotencyKey, out var existing))
        {
            return Task.FromResult(existing);
        }

        var key = (request.AccountId, request.CurrencyCode);
        var currentBalance = _balances.GetOrAdd(key, 0);
        long newBalance;

        if (request.EntryType == "Debit")
        {
            if (currentBalance < request.AmountMinorUnits)
                throw new InvalidOperationException("Insufficient balance");
            newBalance = currentBalance - request.AmountMinorUnits;
        }
        else
        {
            newBalance = currentBalance + request.AmountMinorUnits;
        }

        _balances[key] = newBalance;

        var entry = new LedgerEntryDto
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            EntryType = request.EntryType,
            CurrencyCode = request.CurrencyCode,
            AmountMinorUnits = request.AmountMinorUnits,
            AmountDecimal = request.AmountMinorUnits / 100m,
            Description = request.Description,
            Reference = request.Reference,
            TransactionId = Guid.NewGuid(),
            EntryDate = DateTime.UtcNow,
            RunningBalance = newBalance
        };

        var entries = _entries.GetOrAdd(request.AccountId, _ => new List<LedgerEntryDto>());
        lock (entries)
        {
            entries.Add(entry);
        }

        var response = new PostTransactionResponse
        {
            TransactionId = entry.TransactionId.Value,
            Status = "Posted",
            NewBalanceMinorUnits = newBalance
        };

        if (request.IdempotencyKey != null)
        {
            _idempotencyKeys[request.IdempotencyKey] = response;
        }

        return Task.FromResult(response);
    }

    public Task<PostTransactionResponse> VoidTransactionAsync(VoidTransactionRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would reverse the original transaction
        var response = new PostTransactionResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Voided",
            NewBalanceMinorUnits = 0
        };

        return Task.FromResult(response);
    }

    public Task<PostTransactionResponse> ApplyAdjustmentAsync(AdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        var key = (request.AccountId, request.CurrencyCode);
        var currentBalance = _balances.GetOrAdd(key, 0);
        var newBalance = currentBalance + request.AmountMinorUnits;
        _balances[key] = newBalance;

        var response = new PostTransactionResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Adjusted",
            NewBalanceMinorUnits = newBalance
        };

        return Task.FromResult(response);
    }

    public Task<bool> HasSufficientBalanceAsync(Guid accountId, string currencyCode, long amountMinorUnits, CancellationToken cancellationToken = default)
    {
        var balance = _balances.GetOrAdd((accountId, currencyCode), 0);
        return Task.FromResult(balance >= amountMinorUnits);
    }

    public void SeedBalance(Guid accountId, string currencyCode, long amountMinorUnits)
    {
        _balances[(accountId, currencyCode)] = amountMinorUnits;
    }

    private static int GetNumericCode(string currencyCode) => currencyCode.ToUpper() switch
    {
        "MAD" => 504,
        "EUR" => 978,
        "USD" => 840,
        "GBP" => 826,
        _ => 0
    };
}
