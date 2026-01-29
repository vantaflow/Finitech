using Finitech.Modules.IdentityAccess.Contracts;
using Finitech.Modules.IdentityAccess.Contracts.DTOs;
using Finitech.Modules.IdentityCompliance.Contracts;
using Finitech.Modules.IdentityCompliance.Contracts.DTOs;
using Finitech.Modules.Wallet.Contracts;
using Finitech.Modules.Wallet.Contracts.DTOs;
using Finitech.Modules.WalletFMCG.Contracts;
using Finitech.Modules.WalletFMCG.Contracts.DTOs;
using Finitech.Modules.MerchantPayments.Contracts;
using Finitech.Modules.MerchantPayments.Contracts.DTOs;
using Finitech.Modules.Payments.Contracts;
using Finitech.Modules.Payments.Contracts.DTOs;
using Finitech.Modules.Banking.Contracts;
using Finitech.Modules.Banking.Contracts.DTOs;
using Finitech.Modules.BranchNetwork.Contracts;
using Finitech.Modules.BranchNetwork.Contracts.DTOs;
using Finitech.Modules.Notifications.Contracts;
using Finitech.Modules.Notifications.Contracts.DTOs;
using Finitech.Modules.Statements.Contracts;
using Finitech.Modules.Statements.Contracts.DTOs;
using Finitech.Modules.Disputes.Contracts;
using Finitech.Modules.Disputes.Contracts.DTOs;
using Finitech.Modules.Documents.Contracts;
using Finitech.Modules.Documents.Contracts.DTOs;
using Finitech.Modules.Budgeting.Contracts;
using Finitech.Modules.Budgeting.Contracts.DTOs;
using Finitech.Modules.Audit.Contracts;
using Finitech.Modules.Audit.Contracts.DTOs;
using Finitech.Modules.Scheduler.Contracts;
using Finitech.Modules.Scheduler.Contracts.DTOs;
using System.Collections.Concurrent;
using System.Text;

namespace Finitech.ApiHost.Services;

public class IdentityAccessService : IIdentityAccessService
{
    private readonly ConcurrentDictionary<string, (RegisterResponse Response, string Password)> _users = new();
    private readonly ConcurrentDictionary<string, string> _resetTokens = new();
    private readonly ConcurrentDictionary<string, LoginResponse> _sessions = new();

    public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = new RegisterResponse
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            Status = "Active"
        };
        _users[request.Email.ToLower()] = (response, request.Password);
        return Task.FromResult(response);
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.EmailOrPhone.ToLower();
        if (!_users.TryGetValue(email, out var user) || user.Password != HashPassword(request.Password))
            throw new InvalidOperationException("Invalid credentials");

        var response = new LoginResponse
        {
            AccessToken = GenerateToken(),
            RefreshToken = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            UserId = user.Response.UserId.ToString(),
            Email = user.Response.Email
        };
        _sessions[response.AccessToken] = response;
        return Task.FromResult(response);
    }

    public Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var response = new LoginResponse
        {
            AccessToken = GenerateToken(),
            RefreshToken = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            UserId = Guid.NewGuid().ToString(),
            Email = "refreshed@example.com"
        };
        return Task.FromResult(response);
    }

    public Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var token = GenerateToken();
        _resetTokens[request.Email.ToLower()] = token;
        return Task.FromResult(new ForgotPasswordResponse
        {
            ResetToken = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ChangeContactAsync(Guid userId, ChangeContactRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task LockAccountAsync(Guid userId, string reason, string? adminId = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UnlockAccountAsync(Guid userId, string? adminId = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sessions.ContainsKey(token));
    }

    public Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(token, out var session))
            return Task.FromResult<Guid?>(Guid.Parse(session.UserId));
        return Task.FromResult<Guid?>(null);
    }

    private static string HashPassword(string password) => password;
    private static string GenerateToken() => Guid.NewGuid().ToString("N");
}

public class IdentityComplianceService : IIdentityComplianceService
{
    private readonly ConcurrentDictionary<Guid, KYCDto> _kycs = new();
    private readonly ConcurrentDictionary<Guid, KYBDto> _kybs = new();
    private readonly ConcurrentDictionary<Guid, List<FraudCaseDto>> _fraudCases = new();

    public Task<KYCDto> SubmitKYCAsync(SubmitKYCRequest request, CancellationToken cancellationToken = default)
    {
        var kyc = new KYCDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow
        };
        _kycs[kyc.Id] = kyc;
        return Task.FromResult(kyc);
    }

    public Task<KYCDto> ReviewKYCAsync(Guid kycId, ReviewKYCRequest request, CancellationToken cancellationToken = default)
    {
        if (!_kycs.TryGetValue(kycId, out var kyc))
            throw new InvalidOperationException("KYC not found");

        kyc = kyc with
        {
            Status = request.Decision,
            RejectionReason = request.RejectionReason,
            ReviewedAt = DateTime.UtcNow,
            ReviewedBy = request.ReviewedBy
        };
        _kycs[kycId] = kyc;
        return Task.FromResult(kyc);
    }

    public Task<KYBDto> SubmitKYBAsync(SubmitKYBRequest request, CancellationToken cancellationToken = default)
    {
        var kyb = new KYBDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            BusinessType = request.BusinessType,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow
        };
        _kybs[kyb.Id] = kyb;
        return Task.FromResult(kyb);
    }

    public Task<KYBDto> ReviewKYBAsync(Guid kybId, ReviewKYBRequest request, CancellationToken cancellationToken = default)
    {
        if (!_kybs.TryGetValue(kybId, out var kyb))
            throw new InvalidOperationException("KYB not found");

        kyb = kyb with
        {
            Status = request.Decision,
            RejectionReason = request.RejectionReason,
            ReviewedAt = DateTime.UtcNow
        };
        _kybs[kybId] = kyb;
        return Task.FromResult(kyb);
    }

    public Task<AMLScreeningResultDto> ScreenPartyAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AMLScreeningResultDto
        {
            PartyId = partyId,
            RiskLevel = "Low",
            RiskScore = 10,
            Hits = new List<AMLScreeningHitDto>()
        });
    }

    public Task<KYCDto?> GetKYCStatusAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var kyc = _kycs.Values.FirstOrDefault(k => k.PartyId == partyId);
        return Task.FromResult(kyc);
    }

    public Task<KYBDto?> GetKYBStatusAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var kyb = _kybs.Values.FirstOrDefault(k => k.PartyId == partyId);
        return Task.FromResult(kyb);
    }

    public Task<bool> IsKYCApprovedAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var kyc = _kycs.Values.FirstOrDefault(k => k.PartyId == partyId);
        return Task.FromResult(kyc?.Status == "Approved");
    }

    public Task<bool> IsKYBApprovedAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var kyb = _kybs.Values.FirstOrDefault(k => k.PartyId == partyId);
        return Task.FromResult(kyb?.Status == "Approved");
    }

    public Task<IReadOnlyList<FraudCaseDto>> GetFraudCasesAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var cases = _fraudCases.GetValueOrDefault(partyId, new List<FraudCaseDto>());
        return Task.FromResult<IReadOnlyList<FraudCaseDto>>(cases);
    }

    public Task<FraudCaseDto> ExecuteStrongActionAsync(StrongActionRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FraudCaseDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            CaseType = request.ActionType,
            Status = "Applied",
            RiskLevel = "High",
            Description = request.Reason,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public class WalletService : IWalletService
{
    private readonly ConcurrentDictionary<Guid, WalletAccountDto> _wallets = new();
    private readonly ConcurrentDictionary<Guid, List<WalletLimitsDto>> _limits = new();
    private readonly ConcurrentDictionary<Guid, LoyaltyPointsDto> _loyalty = new();

    public Task<WalletAccountDto> CreateWalletAsync(CreateWalletRequest request, CancellationToken cancellationToken = default)
    {
        var wallet = new WalletAccountDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            WalletLevel = request.InitialLevel,
            Status = "Active",
            Balances = request.SupportedCurrencies.Select(c => new WalletBalanceDto
            {
                CurrencyCode = c,
                BalanceMinorUnits = 0,
                BalanceDecimal = 0
            }).ToList(),
            CreatedAt = DateTime.UtcNow
        };
        _wallets[wallet.Id] = wallet;

        _limits[wallet.Id] = new List<WalletLimitsDto>
        {
            new() { LimitType = "CashIn", CurrencyCode = "MAD", DailyLimitMinorUnits = 1000000, MonthlyLimitMinorUnits = 10000000, DailyUsedMinorUnits = 0, MonthlyUsedMinorUnits = 0 },
            new() { LimitType = "CashOut", CurrencyCode = "MAD", DailyLimitMinorUnits = 500000, MonthlyLimitMinorUnits = 5000000, DailyUsedMinorUnits = 0, MonthlyUsedMinorUnits = 0 },
            new() { LimitType = "P2PSend", CurrencyCode = "MAD", DailyLimitMinorUnits = 500000, MonthlyLimitMinorUnits = 5000000, DailyUsedMinorUnits = 0, MonthlyUsedMinorUnits = 0 }
        };

        _loyalty[wallet.Id] = new LoyaltyPointsDto
        {
            WalletId = wallet.Id,
            AvailablePoints = 0,
            LifetimePoints = 0,
            Tier = "Bronze",
            TierProgress = 0
        };

        return Task.FromResult(wallet);
    }

    public Task<WalletAccountDto?> GetWalletAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        _wallets.TryGetValue(walletId, out var wallet);
        return Task.FromResult(wallet);
    }

    public Task<WalletAccountDto?> GetWalletByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var wallet = _wallets.Values.FirstOrDefault(w => w.PartyId == partyId);
        return Task.FromResult(wallet);
    }

    public Task<IReadOnlyList<WalletLimitsDto>> GetWalletLimitsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var limits = _limits.GetValueOrDefault(walletId, new List<WalletLimitsDto>());
        return Task.FromResult<IReadOnlyList<WalletLimitsDto>>(limits);
    }

    public Task<TransferResultDto> P2PSendAsync(P2PSendRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TransferResultDto
        {
            TransactionId = Guid.NewGuid(),
            Status = "Completed",
            NewBalanceMinorUnits = 450000
        });
    }

    public Task<P2PRequestDto> P2PRequestMoneyAsync(P2PRequestMoneyRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new P2PRequestDto
        {
            Id = Guid.NewGuid(),
            RequesterWalletId = request.FromWalletId,
            TargetWalletId = Guid.NewGuid(),
            CurrencyCode = request.CurrencyCode,
            AmountMinorUnits = request.AmountMinorUnits,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    public Task RespondToP2PRequestAsync(RespondToP2PRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<SplitPaymentDto> CreateSplitPaymentAsync(SplitPaymentRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SplitPaymentDto
        {
            Id = Guid.NewGuid(),
            InitiatorWalletId = request.InitiatorWalletId,
            CurrencyCode = request.CurrencyCode,
            TotalAmountMinorUnits = request.TotalAmountMinorUnits,
            Status = "Pending",
            Participants = request.ParticipantIdentifiers.Select((p, i) => new SplitParticipantDto
            {
                WalletId = Guid.NewGuid(),
                AmountMinorUnits = request.TotalAmountMinorUnits / request.ParticipantIdentifiers.Count,
                HasPaid = false
            }).ToList()
        });
    }

    public Task<SplitPaymentDto> PaySplitShareAsync(Guid splitId, Guid walletId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SplitPaymentDto
        {
            Id = splitId,
            InitiatorWalletId = walletId,
            CurrencyCode = "MAD",
            TotalAmountMinorUnits = 100000,
            Status = "PartiallyPaid",
            Participants = new List<SplitParticipantDto>()
        });
    }

    public Task<ScheduledWalletPaymentDto> CreateScheduledPaymentAsync(CreateScheduledPaymentRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ScheduledWalletPaymentDto
        {
            Id = Guid.NewGuid(),
            WalletId = request.WalletId,
            PaymentType = request.PaymentType,
            CurrencyCode = request.CurrencyCode,
            AmountMinorUnits = request.AmountMinorUnits,
            Frequency = request.Frequency,
            NextExecutionAt = request.StartDate,
            Status = "Active"
        });
    }

    public Task<IReadOnlyList<ScheduledWalletPaymentDto>> GetScheduledPaymentsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ScheduledWalletPaymentDto>>(new List<ScheduledWalletPaymentDto>());
    }

    public Task CancelScheduledPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<LoyaltyPointsDto> GetLoyaltyPointsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var loyalty = _loyalty.GetValueOrDefault(walletId, new LoyaltyPointsDto
        {
            WalletId = walletId,
            AvailablePoints = 0,
            LifetimePoints = 0,
            Tier = "Bronze",
            TierProgress = 0
        });
        return Task.FromResult(loyalty);
    }

    public Task<IReadOnlyList<LoyaltyTransactionDto>> GetLoyaltyTransactionsAsync(Guid walletId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<LoyaltyTransactionDto>>(new List<LoyaltyTransactionDto>());
    }

    public Task<RedeemResultDto> RedeemPointsAsync(RedeemPointsRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RedeemResultDto
        {
            PointsRedeemed = request.Points,
            AmountCreditedMinorUnits = request.Points * 10,
            CurrencyCode = "MAD"
        });
    }

    public Task<NFCTokenDto> GenerateNFCTokenAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NFCTokenDto
        {
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Status = "Active"
        });
    }

    public Task RevokeNFCTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class WalletFMCGService : IWalletFMCGService
{
    private readonly ConcurrentDictionary<Guid, FloatAccountDto> _floats = new();
    private readonly ConcurrentDictionary<Guid, List<CommissionDto>> _commissions = new();

    public Task<FloatAccountDto> CreateFloatAccountAsync(Guid partyId, string accountType, CancellationToken cancellationToken = default)
    {
        var floatAccount = new FloatAccountDto
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            AccountType = accountType,
            Status = "Active",
            Balances = new List<FloatBalanceDto>
            {
                new() { CurrencyCode = "MAD", BalanceMinorUnits = 1000000, BalanceDecimal = 10000 }
            },
            MinBalanceAlertThreshold = 100000,
            CreatedAt = DateTime.UtcNow
        };
        _floats[floatAccount.Id] = floatAccount;
        return Task.FromResult(floatAccount);
    }

    public Task<FloatAccountDto?> GetFloatAccountAsync(Guid floatAccountId, CancellationToken cancellationToken = default)
    {
        _floats.TryGetValue(floatAccountId, out var floatAccount);
        return Task.FromResult(floatAccount);
    }

    public Task<FloatAccountDto?> GetFloatAccountByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var floatAccount = _floats.Values.FirstOrDefault(f => f.PartyId == partyId);
        return Task.FromResult(floatAccount);
    }

    public Task<CashOperationResultDto> CashInAsync(CashInRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CashOperationResultDto
        {
            OperationId = Guid.NewGuid(),
            OperationType = "CashIn",
            Status = "Completed",
            AgentNewBalanceMinorUnits = 1100000,
            CustomerNewBalanceMinorUnits = 550000,
            ExecutedAt = DateTime.UtcNow
        });
    }

    public Task<CashOperationResultDto> CashOutAsync(CashOutRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CashOperationResultDto
        {
            OperationId = Guid.NewGuid(),
            OperationType = "CashOut",
            Status = "Completed",
            AgentNewBalanceMinorUnits = 900000,
            CustomerNewBalanceMinorUnits = 450000,
            ExecutedAt = DateTime.UtcNow
        });
    }

    public Task<NetworkHierarchyDto> GetNetworkHierarchyAsync(Guid distributorId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NetworkHierarchyDto
        {
            DistributorId = distributorId,
            DistributorName = "Distrib Express",
            AgentCount = 5,
            MerchantCount = 25,
            Agents = new List<AgentInNetworkDto>()
        });
    }

    public Task AssignAgentToDistributorAsync(Guid agentId, Guid distributorId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task AssignMerchantToAgentAsync(Guid merchantId, Guid agentId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CommissionDto>> GetCommissionsAsync(Guid beneficiaryId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var commissions = _commissions.GetValueOrDefault(beneficiaryId, new List<CommissionDto>())
            .Where(c => c.CalculatedAt >= fromDate && c.CalculatedAt <= toDate)
            .ToList();
        return Task.FromResult<IReadOnlyList<CommissionDto>>(commissions);
    }

    public Task<CommissionDto> CalculateCommissionAsync(Guid beneficiaryId, string beneficiaryType, string operationType, string currencyCode, long amountMinorUnits, Guid originalTransactionId, CancellationToken cancellationToken = default)
    {
        var commissionRate = 0.01m; // 1% commission
        var commissionAmount = (long)(amountMinorUnits * commissionRate);

        var commission = new CommissionDto
        {
            Id = Guid.NewGuid(),
            BeneficiaryId = beneficiaryId,
            BeneficiaryType = beneficiaryType,
            OperationType = operationType,
            CurrencyCode = currencyCode,
            AmountMinorUnits = amountMinorUnits,
            CommissionRate = commissionRate,
            CommissionAmountMinorUnits = commissionAmount,
            OriginalTransactionId = originalTransactionId,
            CalculatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        var list = _commissions.GetOrAdd(beneficiaryId, _ => new List<CommissionDto>());
        lock (list) { list.Add(commission); }

        return Task.FromResult(commission);
    }

    public Task SetCommissionRuleAsync(CommissionRuleDto rule, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CommissionRuleDto>> GetCommissionRulesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<CommissionRuleDto>>(new List<CommissionRuleDto>
        {
            new() { OperationType = "CashIn", BeneficiaryType = "Agent", CommissionRate = 0.005m, IsActive = true },
            new() { OperationType = "CashOut", BeneficiaryType = "Agent", CommissionRate = 0.01m, IsActive = true },
            new() { OperationType = "MerchantPay", BeneficiaryType = "Agent", CommissionRate = 0.005m, IsActive = true }
        });
    }

    public Task<IReadOnlyList<FloatAlertDto>> GetFloatAlertsAsync(Guid floatAccountId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FloatAlertDto>>(new List<FloatAlertDto>());
    }
}

public class MerchantPaymentsService : IMerchantPaymentsService
{
    private readonly ConcurrentDictionary<Guid, MerchantDto> _merchants = new();
    private readonly ConcurrentDictionary<string, DynamicQRDto> _qrs = new();

    public Task<MerchantDto> CreateMerchantAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default)
    {
        var merchant = new MerchantDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            BusinessName = request.BusinessName,
            MerchantCode = request.MerchantCode ?? $"MERCH{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Category = request.Category,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        _merchants[merchant.Id] = merchant;
        return Task.FromResult(merchant);
    }

    public Task<MerchantDto?> GetMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _merchants.TryGetValue(merchantId, out var merchant);
        return Task.FromResult(merchant);
    }

    public Task<MerchantDto?> GetMerchantByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var merchant = _merchants.Values.FirstOrDefault(m => m.PartyId == partyId);
        return Task.FromResult(merchant);
    }

    public Task SuspendMerchantAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default)
    {
        if (_merchants.TryGetValue(merchantId, out var merchant))
            _merchants[merchantId] = merchant with { Status = "Suspended" };
        return Task.CompletedTask;
    }

    public Task ActivateMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        if (_merchants.TryGetValue(merchantId, out var merchant))
            _merchants[merchantId] = merchant with { Status = "Active" };
        return Task.CompletedTask;
    }

    public Task<DynamicQRDto> GenerateDynamicQRAsync(GenerateDynamicQRRequest request, CancellationToken cancellationToken = default)
    {
        var currencyNumericCode = request.CurrencyCode.ToUpper() switch
        {
            "MAD" => "504",
            "EUR" => "978",
            "USD" => "840",
            _ => "504"
        };

        var amount = request.AmountMinorUnits / 100m;
        var reference = request.Reference ?? Guid.NewGuid().ToString("N")[..12].ToUpper();

        // Simplified EMVCo QR payload
        var payload = $"000201010212{GetMerchantAccountInfo(request.MerchantId)}520441115303{currencyNumericCode}540{amount.ToString("0.00").Length}{amount:0.00}5802MA5912{GetMerchantName(request.MerchantId)}6008CASABLANCA6240{GetAdditionalData(reference)}6304";
        var crc = CalculateCRC(payload);

        var qr = new DynamicQRDto
        {
            Payload = payload + crc,
            PayloadFormat = "EMVCo",
            PayloadLength = payload.Length + 4,
            CurrencyNumericCode = currencyNumericCode,
            Amount = amount,
            Reference = reference,
            ExpiresAt = request.ExpiresAt,
            CRC = crc
        };

        _qrs[reference] = qr;
        return Task.FromResult(qr);
    }

    public Task<ParsedQRDto> ParseQRAsync(string payload, CancellationToken cancellationToken = default)
    {
        // Simplified parsing - in real implementation would parse TLV format
        var isValid = payload.Length <= 512 && payload.StartsWith("000201");
        var crc = payload.Length >= 4 ? payload[^4..] : "";

        return Task.FromResult(new ParsedQRDto
        {
            IsValid = isValid,
            PayloadFormatIndicator = "01",
            PointOfInitiationMethod = "12",
            TransactionCurrency = "504",
            TransactionAmount = 100.00m,
            CRC = crc,
            CrcValid = ValidateCRC(payload)
        });
    }

    public Task<PayByQRResponse> PayByQRAsync(PayByQRRequest request, CancellationToken cancellationToken = default)
    {
        var parsed = ParseQRAsync(request.QRPayload, cancellationToken).Result;
        if (!parsed.IsValid)
            throw new InvalidOperationException("Invalid QR code");

        return Task.FromResult(new PayByQRResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Completed",
            MerchantName = parsed.MerchantName ?? "Unknown Merchant",
            CurrencyCode = "MAD",
            AmountMinorUnits = (long)((parsed.TransactionAmount ?? 0) * 100),
            ExecutedAt = DateTime.UtcNow
        });
    }

    private static string GetMerchantAccountInfo(Guid merchantId) => $"1504{merchantId.ToString()[..4].ToUpper()}0104{merchantId.ToString()[4..8].ToUpper()}";
    private static string GetMerchantName(Guid merchantId) => "MERCHANT";
    private static string GetAdditionalData(string reference) => $"0505{reference}";
    private static string CalculateCRC(string payload) => "A1B2";
    private static bool ValidateCRC(string payload) => true;
}

public class PaymentsService : IPaymentsService
{
    private readonly ConcurrentDictionary<Guid, BeneficiaryDto> _beneficiaries = new();
    private readonly ConcurrentDictionary<Guid, StandingOrderDto> _standingOrders = new();
    private readonly ConcurrentDictionary<Guid, DirectDebitMandateDto> _mandates = new();
    private readonly ConcurrentDictionary<Guid, ScheduledPaymentDto> _scheduledPayments = new();
    private readonly ConcurrentDictionary<string, TransferResponse> _idempotencyKeys = new();

    public Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IdempotencyKey != null && _idempotencyKeys.TryGetValue(request.IdempotencyKey, out var existing))
            return Task.FromResult(existing);

        var response = new TransferResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Completed",
            ExecutedAt = DateTime.UtcNow
        };

        if (request.IdempotencyKey != null)
            _idempotencyKeys[request.IdempotencyKey] = response;

        return Task.FromResult(response);
    }

    public Task<TransferResponse> CrossCurrencyTransferAsync(CrossCurrencyTransferRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TransferResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Completed",
            ExecutedAt = DateTime.UtcNow
        });
    }

    public Task<TransferResponse> PayBillAsync(BillPayRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TransferResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Completed",
            ExecutedAt = DateTime.UtcNow
        });
    }

    public Task<TransferResponse> TopUpAsync(TopUpRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TransferResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "Completed",
            ExecutedAt = DateTime.UtcNow
        });
    }

    public Task<BeneficiaryDto> CreateBeneficiaryAsync(CreateBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        var beneficiary = new BeneficiaryDto
        {
            Id = Guid.NewGuid(),
            OwnerPartyId = request.OwnerPartyId,
            Name = request.Name,
            BeneficiaryType = request.BeneficiaryType,
            Identifier = request.Identifier,
            BankName = request.BankName,
            IsFavorite = false,
            CreatedAt = DateTime.UtcNow
        };
        _beneficiaries[beneficiary.Id] = beneficiary;
        return Task.FromResult(beneficiary);
    }

    public Task<IReadOnlyList<BeneficiaryDto>> GetBeneficiariesAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var beneficiaries = _beneficiaries.Values.Where(b => b.OwnerPartyId == partyId).ToList();
        return Task.FromResult<IReadOnlyList<BeneficiaryDto>>(beneficiaries);
    }

    public Task DeleteBeneficiaryAsync(Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        _beneficiaries.TryRemove(beneficiaryId, out _);
        return Task.CompletedTask;
    }

    public Task<StandingOrderDto> CreateStandingOrderAsync(CreateStandingOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new StandingOrderDto
        {
            Id = Guid.NewGuid(),
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            CurrencyCode = request.CurrencyCode,
            AmountMinorUnits = request.AmountMinorUnits,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Active",
            NextExecutionAt = request.StartDate
        };
        _standingOrders[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<StandingOrderDto>> GetStandingOrdersAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var orders = _standingOrders.Values.Where(o => o.FromAccountId == accountId).ToList();
        return Task.FromResult<IReadOnlyList<StandingOrderDto>>(orders);
    }

    public Task CancelStandingOrderAsync(Guid standingOrderId, CancellationToken cancellationToken = default)
    {
        if (_standingOrders.TryGetValue(standingOrderId, out var order))
            _standingOrders[standingOrderId] = order with { Status = "Cancelled" };
        return Task.CompletedTask;
    }

    public Task<DirectDebitMandateDto> CreateDirectDebitMandateAsync(CreateDirectDebitMandateRequest request, CancellationToken cancellationToken = default)
    {
        var mandate = new DirectDebitMandateDto
        {
            Id = Guid.NewGuid(),
            DebtorAccountId = request.DebtorAccountId,
            CreditorName = request.CreditorName,
            CreditorIdentifier = request.CreditorIdentifier,
            IBAN = request.IBAN,
            MaxAmount = request.MaxAmount,
            Frequency = request.Frequency,
            Status = "Active",
            SignedAt = DateTime.UtcNow
        };
        _mandates[mandate.Id] = mandate;
        return Task.FromResult(mandate);
    }

    public Task<IReadOnlyList<DirectDebitMandateDto>> GetDirectDebitMandatesAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var mandates = _mandates.Values.Where(m => m.DebtorAccountId == accountId).ToList();
        return Task.FromResult<IReadOnlyList<DirectDebitMandateDto>>(mandates);
    }

    public Task CancelDirectDebitMandateAsync(Guid mandateId, CancellationToken cancellationToken = default)
    {
        if (_mandates.TryGetValue(mandateId, out var mandate))
            _mandates[mandateId] = mandate with { Status = "Cancelled" };
        return Task.CompletedTask;
    }

    public Task<ScheduledPaymentDto> SchedulePaymentAsync(SchedulePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = new ScheduledPaymentDto
        {
            Id = Guid.NewGuid(),
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            CurrencyCode = request.CurrencyCode,
            AmountMinorUnits = request.AmountMinorUnits,
            ScheduledFor = request.ScheduledFor,
            Status = "Pending"
        };
        _scheduledPayments[payment.Id] = payment;
        return Task.FromResult(payment);
    }

    public Task<IReadOnlyList<ScheduledPaymentDto>> GetScheduledPaymentsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var payments = _scheduledPayments.Values.Where(p => p.FromAccountId == accountId).ToList();
        return Task.FromResult<IReadOnlyList<ScheduledPaymentDto>>(payments);
    }

    public Task CancelScheduledPaymentAsync(Guid scheduledPaymentId, CancellationToken cancellationToken = default)
    {
        if (_scheduledPayments.TryGetValue(scheduledPaymentId, out var payment))
            _scheduledPayments[scheduledPaymentId] = payment with { Status = "Cancelled" };
        return Task.CompletedTask;
    }
}

public class BankingService : IBankingService
{
    private readonly ConcurrentDictionary<Guid, BankAccountDto> _accounts = new();
    private readonly ConcurrentDictionary<Guid, SavingsAccountDto> _savings = new();
    private readonly ConcurrentDictionary<Guid, FixedDepositDto> _fixedDeposits = new();
    private readonly ConcurrentDictionary<Guid, LoanDto> _loans = new();
    private readonly ConcurrentDictionary<Guid, OverdraftDto> _overdrafts = new();
    private readonly ConcurrentDictionary<Guid, ChequeBookDto> _chequeBooks = new();
    private readonly ConcurrentDictionary<Guid, ChequeDepositDto> _cheques = new();
    private readonly ConcurrentDictionary<Guid, CardDto> _cards = new();
    private int _accountCounter = 123456789;

    public Task<BankAccountDto> CreateAccountAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new BankAccountDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            AccountNumber = request.AccountNumber ?? $"007{_accountCounter++.ToString().PadLeft(18, '0')}",
            AccountType = request.AccountType,
            Status = "Active",
            CurrencyCode = request.CurrencyCode,
            BalanceMinorUnits = 0,
            BalanceDecimal = 0,
            OpenedAt = DateTime.UtcNow
        };
        _accounts[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<BankAccountDto?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        _accounts.TryGetValue(accountId, out var account);
        return Task.FromResult(account);
    }

    public Task<IReadOnlyList<BankAccountDto>> GetAccountsByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var accounts = _accounts.Values.Where(a => a.PartyId == partyId).ToList();
        return Task.FromResult<IReadOnlyList<BankAccountDto>>(accounts);
    }

    public Task CloseAccountAsync(Guid accountId, string reason, CancellationToken cancellationToken = default)
    {
        if (_accounts.TryGetValue(accountId, out var account))
            _accounts[accountId] = account with { Status = "Closed" };
        return Task.CompletedTask;
    }

    public Task<SavingsAccountDto> CreateSavingsAccountAsync(CreateSavingsAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new SavingsAccountDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            AccountNumber = $"007{_accountCounter++.ToString().PadLeft(18, '0')}",
            AccountType = "Savings",
            Status = "Active",
            CurrencyCode = request.CurrencyCode,
            BalanceMinorUnits = request.InitialDepositMinorUnits,
            BalanceDecimal = request.InitialDepositMinorUnits / 100m,
            InterestRate = request.InterestRate,
            MinimumBalanceMinorUnits = request.MinimumBalanceMinorUnits,
            AccruedInterestMinorUnits = 0,
            OpenedAt = DateTime.UtcNow
        };
        _savings[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task DepositAsync(DepositRequest request, CancellationToken cancellationToken = default)
    {
        if (_savings.TryGetValue(request.AccountId, out var account))
        {
            var newBalance = account.BalanceMinorUnits + request.AmountMinorUnits;
            _savings[request.AccountId] = account with
            {
                BalanceMinorUnits = newBalance,
                BalanceDecimal = newBalance / 100m
            };
        }
        return Task.CompletedTask;
    }

    public Task WithdrawAsync(WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        if (_savings.TryGetValue(request.AccountId, out var account))
        {
            var newBalance = account.BalanceMinorUnits - request.AmountMinorUnits;
            _savings[request.AccountId] = account with
            {
                BalanceMinorUnits = newBalance,
                BalanceDecimal = newBalance / 100m
            };
        }
        return Task.CompletedTask;
    }

    public Task CalculateInterestAsync(Guid savingsAccountId, CancellationToken cancellationToken = default)
    {
        if (_savings.TryGetValue(savingsAccountId, out var account))
        {
            var dailyInterest = (long)(account.BalanceMinorUnits * account.InterestRate / 365);
            var newAccrued = account.AccruedInterestMinorUnits + dailyInterest;
            _savings[savingsAccountId] = account with { AccruedInterestMinorUnits = newAccrued };
        }
        return Task.CompletedTask;
    }

    public Task<FixedDepositDto> CreateFixedDepositAsync(CreateFixedDepositRequest request, CancellationToken cancellationToken = default)
    {
        var maturityAmount = (long)(request.PrincipalAmountMinorUnits * (1 + request.InterestRate * request.DurationMonths / 12));
        var fd = new FixedDepositDto
        {
            Id = Guid.NewGuid(),
            AccountId = request.SourceAccountId,
            AccountNumber = $"FD{_accountCounter++.ToString().PadLeft(10, '0')}",
            PrincipalAmountMinorUnits = request.PrincipalAmountMinorUnits,
            InterestRate = request.InterestRate,
            DurationMonths = request.DurationMonths,
            StartDate = DateTime.UtcNow,
            MaturityDate = DateTime.UtcNow.AddMonths(request.DurationMonths),
            MaturityAmountMinorUnits = maturityAmount,
            AccruedInterestMinorUnits = 0,
            AutoRenewal = request.AutoRenewal,
            Status = "Active"
        };
        _fixedDeposits[fd.Id] = fd;
        return Task.FromResult(fd);
    }

    public Task<FixedDepositDto?> GetFixedDepositAsync(Guid fixedDepositId, CancellationToken cancellationToken = default)
    {
        _fixedDeposits.TryGetValue(fixedDepositId, out var fd);
        return Task.FromResult(fd);
    }

    public Task WithdrawFixedDepositEarlyAsync(Guid fixedDepositId, CancellationToken cancellationToken = default)
    {
        if (_fixedDeposits.TryGetValue(fixedDepositId, out var fd))
            _fixedDeposits[fixedDepositId] = fd with { Status = "WithdrawnEarly" };
        return Task.CompletedTask;
    }

    public Task ProcessMaturedFixedDepositsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var fd in _fixedDeposits.Values.Where(f => f.MaturityDate <= DateTime.UtcNow && f.Status == "Active"))
            _fixedDeposits[fd.Id] = fd with { Status = "Matured" };
        return Task.CompletedTask;
    }

    public Task<LoanDto> RequestLoanAsync(LoanApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var loan = new LoanDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            LoanNumber = $"LN{_accountCounter++.ToString().PadLeft(10, '0')}",
            PrincipalAmountMinorUnits = request.RequestedAmountMinorUnits,
            InterestRate = 0.05m,
            DurationMonths = request.RequestedDurationMonths,
            MonthlyPaymentMinorUnits = request.RequestedAmountMinorUnits / request.RequestedDurationMonths,
            RemainingBalanceMinorUnits = request.RequestedAmountMinorUnits,
            Status = "Pending",
            DisbursedAt = DateTime.UtcNow,
            RemainingInstallments = request.RequestedDurationMonths
        };
        _loans[loan.Id] = loan;
        return Task.FromResult(loan);
    }

    public Task<LoanDto> ApproveLoanAsync(LoanApprovalRequest request, CancellationToken cancellationToken = default)
    {
        if (!_loans.TryGetValue(request.LoanId, out var loan))
            throw new InvalidOperationException("Loan not found");

        _loans[request.LoanId] = loan with
        {
            Status = request.Approved ? "Active" : "Rejected",
            InterestRate = request.ApprovedInterestRate ?? loan.InterestRate
        };
        return Task.FromResult(_loans[request.LoanId]);
    }

    public Task DisburseLoanAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        if (_loans.TryGetValue(loanId, out var loan))
            _loans[loanId] = loan with { Status = "Active", DisbursedAt = DateTime.UtcNow };
        return Task.CompletedTask;
    }

    public Task RepayLoanAsync(LoanRepaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (_loans.TryGetValue(request.LoanId, out var loan))
        {
            var newBalance = loan.RemainingBalanceMinorUnits - request.AmountMinorUnits;
            _loans[request.LoanId] = loan with
            {
                RemainingBalanceMinorUnits = newBalance,
                RemainingInstallments = loan.RemainingInstallments - 1,
                Status = newBalance <= 0 ? "Closed" : loan.Status
            };
        }
        return Task.CompletedTask;
    }

    public Task<LoanDto?> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        _loans.TryGetValue(loanId, out var loan);
        return Task.FromResult(loan);
    }

    public Task<IReadOnlyList<LoanDto>> GetLoansByPartyIdAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var loans = _loans.Values.Where(l => l.PartyId == partyId).ToList();
        return Task.FromResult<IReadOnlyList<LoanDto>>(loans);
    }

    public Task<OverdraftDto> SetOverdraftAsync(SetOverdraftRequest request, CancellationToken cancellationToken = default)
    {
        var overdraft = new OverdraftDto
        {
            AccountId = request.AccountId,
            ApprovedLimitMinorUnits = request.LimitMinorUnits,
            CurrentOverdraftMinorUnits = 0,
            InterestRate = request.InterestRate,
            AccruedFeesMinorUnits = 0,
            IsActive = true
        };
        _overdrafts[request.AccountId] = overdraft;
        return Task.FromResult(overdraft);
    }

    public Task<OverdraftDto?> GetOverdraftAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        _overdrafts.TryGetValue(accountId, out var overdraft);
        return Task.FromResult(overdraft);
    }

    public Task CalculateOverdraftFeesAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        if (_overdrafts.TryGetValue(accountId, out var overdraft) && overdraft.CurrentOverdraftMinorUnits > 0)
        {
            var dailyFee = (long)(overdraft.CurrentOverdraftMinorUnits * overdraft.InterestRate / 365);
            _overdrafts[accountId] = overdraft with { AccruedFeesMinorUnits = overdraft.AccruedFeesMinorUnits + dailyFee };
        }
        return Task.CompletedTask;
    }

    public Task<ChequeBookDto> OrderChequeBookAsync(OrderChequeBookRequest request, CancellationToken cancellationToken = default)
    {
        var start = _accountCounter;
        var book = new ChequeBookDto
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            BookNumber = $"CB{_accountCounter++.ToString().PadLeft(8, '0')}",
            StartNumber = start,
            EndNumber = start + request.NumberOfCheques - 1,
            RemainingCheques = request.NumberOfCheques,
            Status = "Active",
            OrderedAt = DateTime.UtcNow
        };
        _chequeBooks[book.Id] = book;
        return Task.FromResult(book);
    }

    public Task<IReadOnlyList<ChequeBookDto>> GetChequeBooksAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var books = _chequeBooks.Values.Where(b => b.AccountId == accountId).ToList();
        return Task.FromResult<IReadOnlyList<ChequeBookDto>>(books);
    }

    public Task CancelChequeBookAsync(Guid chequeBookId, CancellationToken cancellationToken = default)
    {
        if (_chequeBooks.TryGetValue(chequeBookId, out var book))
            _chequeBooks[chequeBookId] = book with { Status = "Cancelled" };
        return Task.CompletedTask;
    }

    public Task<ChequeDepositDto> DepositChequeAsync(DepositChequeRequest request, CancellationToken cancellationToken = default)
    {
        var deposit = new ChequeDepositDto
        {
            Id = Guid.NewGuid(),
            ToAccountId = request.ToAccountId,
            ChequeNumber = request.ChequeNumber,
            DraweeBank = request.DraweeBank,
            AmountMinorUnits = request.AmountMinorUnits,
            Status = "PendingClearance",
            DepositedAt = DateTime.UtcNow
        };
        _cheques[deposit.Id] = deposit;
        return Task.FromResult(deposit);
    }

    public Task ProcessChequeClearanceAsync(Guid chequeDepositId, CancellationToken cancellationToken = default)
    {
        if (_cheques.TryGetValue(chequeDepositId, out var cheque))
            _cheques[chequeDepositId] = cheque with { Status = "Cleared", ClearedAt = DateTime.UtcNow };
        return Task.CompletedTask;
    }

    public Task RejectChequeAsync(Guid chequeDepositId, string reason, CancellationToken cancellationToken = default)
    {
        if (_cheques.TryGetValue(chequeDepositId, out var cheque))
            _cheques[chequeDepositId] = cheque with { Status = "Rejected" };
        return Task.CompletedTask;
    }

    public Task<CardDto> GetCardAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        _cards.TryGetValue(cardId, out var card);
        return Task.FromResult(card ?? new CardDto
        {
            Id = cardId,
            AccountId = Guid.NewGuid(),
            CardNumberMasked = "**** **** **** 1234",
            CardType = "Debit",
            Status = "Active",
            ExpiryDate = DateTime.UtcNow.AddYears(3),
            DailyWithdrawalLimitMinorUnits = 50000,
            DailyPaymentLimitMinorUnits = 100000,
            EcommerceLimitMinorUnits = 50000
        });
    }

    public Task<IReadOnlyList<CardDto>> GetCardsByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var cards = _cards.Values.Where(c => c.AccountId == accountId).ToList();
        return Task.FromResult<IReadOnlyList<CardDto>>(cards);
    }

    public Task UpdateCardLimitsAsync(UpdateCardLimitsRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task FreezeCardAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        if (_cards.TryGetValue(cardId, out var card))
            _cards[cardId] = card with { Status = "Frozen" };
        return Task.CompletedTask;
    }

    public Task UnfreezeCardAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        if (_cards.TryGetValue(cardId, out var card))
            _cards[cardId] = card with { Status = "Active" };
        return Task.CompletedTask;
    }

    public Task BlockCardAsync(Guid cardId, string reason, CancellationToken cancellationToken = default)
    {
        if (_cards.TryGetValue(cardId, out var card))
            _cards[cardId] = card with { Status = "Blocked" };
        return Task.CompletedTask;
    }
}

public class BranchNetworkService : IBranchNetworkService
{
    private readonly ConcurrentDictionary<Guid, BranchDto> _branches = new();

    public Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var branch = new BranchDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Services = request.Services,
            WorkingHours = request.WorkingHours,
            Status = "Active"
        };
        _branches[branch.Id] = branch;
        return Task.FromResult(branch);
    }

    public Task<BranchDto?> GetBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        _branches.TryGetValue(branchId, out var branch);
        return Task.FromResult(branch);
    }

    public Task<BranchDto?> GetBranchByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var branch = _branches.Values.FirstOrDefault(b => b.Code == code);
        return Task.FromResult(branch);
    }

    public Task<IReadOnlyList<BranchDto>> ListBranchesAsync(string? city = null, string? service = null, CancellationToken cancellationToken = default)
    {
        var branches = _branches.Values
            .Where(b => city == null || b.Address.City.Contains(city, StringComparison.OrdinalIgnoreCase))
            .Where(b => service == null || b.Services.Contains(service))
            .ToList();
        return Task.FromResult<IReadOnlyList<BranchDto>>(branches);
    }

    public Task<IReadOnlyList<BranchSearchResultDto>> FindNearbyAsync(FindNearbyRequest request, CancellationToken cancellationToken = default)
    {
        var results = _branches.Values
            .Where(b => b.Latitude.HasValue && b.Longitude.HasValue)
            .Select(b => new BranchSearchResultDto
            {
                Branch = b,
                DistanceKm = CalculateDistance(request.Latitude, request.Longitude, b.Latitude!.Value, b.Longitude!.Value)
            })
            .Where(r => r.DistanceKm <= request.RadiusKm)
            .OrderBy(r => r.DistanceKm)
            .Take(request.MaxResults)
            .ToList();

        return Task.FromResult<IReadOnlyList<BranchSearchResultDto>>(results);
    }

    public Task UpdateBranchAsync(Guid branchId, CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CloseBranchAsync(Guid branchId, string reason, CancellationToken cancellationToken = default)
    {
        if (_branches.TryGetValue(branchId, out var branch))
            _branches[branchId] = branch with { Status = "Closed" };
        return Task.CompletedTask;
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}

public class NotificationsService : INotificationsService
{
    private readonly ConcurrentDictionary<Guid, List<NotificationDto>> _notifications = new();

    public Task SendNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new NotificationDto
        {
            Id = Guid.NewGuid(),
            RecipientPartyId = request.RecipientPartyId,
            NotificationType = request.NotificationType,
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body,
            Status = "Sent",
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow
        };

        var list = _notifications.GetOrAdd(request.RecipientPartyId, _ => new List<NotificationDto>());
        lock (list) { list.Add(notification); }

        Console.WriteLine($"[{request.Channel}] To {request.RecipientPartyId}: {request.Subject}");
        return Task.CompletedTask;
    }

    public Task SendTemplatedNotificationAsync(SendTemplatedNotificationRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(Guid partyId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var notifications = _notifications.GetValueOrDefault(partyId, new List<NotificationDto>())
            .Skip(skip)
            .Take(take)
            .ToList();
        return Task.FromResult<IReadOnlyList<NotificationDto>>(notifications);
    }

    public Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdatePreferenceAsync(UpdateNotificationPreferenceRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<NotificationPreferenceDto>>(new List<NotificationPreferenceDto>());
    }
}

public class StatementsService : IStatementsService
{
    public Task<StatementDto> GenerateStatementAsync(GenerateStatementRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new StatementDto
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            StatementType = request.StatementType,
            CurrencyCode = request.CurrencyCode,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            OpeningBalanceMinorUnits = 1000000,
            ClosingBalanceMinorUnits = 1200000,
            OpeningBalanceDecimal = 10000m,
            ClosingBalanceDecimal = 12000m,
            TotalCreditsMinorUnits = 500000,
            TotalDebitsMinorUnits = 300000,
            Transactions = new List<StatementTransactionDto>(),
            GeneratedAt = DateTime.UtcNow,
            Status = "Generated"
        });
    }

    public Task<StatementDto?> GetStatementAsync(Guid statementId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<StatementDto?>(null);
    }

    public Task<IReadOnlyList<StatementDto>> GetStatementsByAccountAsync(Guid accountId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<StatementDto>>(new List<StatementDto>());
    }

    public Task<ConsolidatedStatementDto> GenerateConsolidatedStatementAsync(Guid partyId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ConsolidatedStatementDto
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            FromDate = fromDate,
            ToDate = toDate,
            Sections = new List<StatementSectionDto>(),
            GeneratedAt = DateTime.UtcNow
        });
    }

    public Task<StatementExportDto> ExportStatementAsync(StatementExportRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new StatementExportDto
        {
            StatementId = request.StatementId,
            Format = request.Format,
            Content = Encoding.UTF8.GetBytes("Statement content"),
            ContentType = "application/pdf",
            FileName = $"statement_{request.StatementId}.pdf"
        });
    }

    public Task GenerateMonthlyStatementsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Generating monthly statements...");
        return Task.CompletedTask;
    }
}

public class DisputesService : IDisputesService
{
    private readonly ConcurrentDictionary<Guid, ChargebackDto> _chargebacks = new();

    public Task<RefundResponse> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RefundResponse
        {
            RefundId = Guid.NewGuid(),
            LedgerEntryId = Guid.NewGuid(),
            Status = "Completed",
            RefundedAmountMinorUnits = request.AmountMinorUnits ?? 100000,
            ExecutedAt = DateTime.UtcNow
        });
    }

    public Task<ChargebackDto> InitiateChargebackAsync(ChargebackRequest request, CancellationToken cancellationToken = default)
    {
        var chargeback = new ChargebackDto
        {
            Id = Guid.NewGuid(),
            OriginalTransactionId = request.OriginalTransactionId,
            Status = "Initiated",
            Reason = request.Reason,
            InitiatedAt = DateTime.UtcNow
        };
        _chargebacks[chargeback.Id] = chargeback;
        return Task.FromResult(chargeback);
    }

    public Task<RepresentmentDto> SubmitRepresentmentAsync(RepresentmentRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RepresentmentDto
        {
            Id = Guid.NewGuid(),
            ChargebackId = request.ChargebackId,
            Evidence = request.Evidence,
            EvidenceDocumentUrls = request.EvidenceDocumentUrls,
            SubmittedAt = DateTime.UtcNow,
            Status = "Submitted"
        });
    }

    public Task<ChargebackDto> ResolveChargebackAsync(ResolveChargebackRequest request, CancellationToken cancellationToken = default)
    {
        if (!_chargebacks.TryGetValue(request.ChargebackId, out var chargeback))
            throw new InvalidOperationException("Chargeback not found");

        _chargebacks[request.ChargebackId] = chargeback with
        {
            Status = request.Resolution,
            Resolution = request.Notes,
            ResolvedAt = DateTime.UtcNow
        };
        return Task.FromResult(_chargebacks[request.ChargebackId]);
    }

    public Task<ChargebackDto?> GetChargebackAsync(Guid chargebackId, CancellationToken cancellationToken = default)
    {
        _chargebacks.TryGetValue(chargebackId, out var chargeback);
        return Task.FromResult(chargeback);
    }

    public Task<IReadOnlyList<ChargebackDto>> GetChargebacksByTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var chargebacks = _chargebacks.Values.Where(c => c.OriginalTransactionId == transactionId).ToList();
        return Task.FromResult<IReadOnlyList<ChargebackDto>>(chargebacks);
    }
}

public class DocumentsService : IDocumentsService
{
    private readonly ConcurrentDictionary<Guid, DocumentDto> _documents = new();

    public Task<DocumentDto> UploadAsync(UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var doc = new DocumentDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            DocumentType = request.DocumentType,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSizeBytes = request.Content.Length,
            Version = 1,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            Metadata = request.Metadata ?? new Dictionary<string, string>(),
            UploadedAt = DateTime.UtcNow,
            UploadedBy = "system",
            IsDeleted = false
        };
        _documents[doc.Id] = doc;
        return Task.FromResult(doc);
    }

    public Task<DocumentContentDto> GetContentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DocumentContentDto
        {
            Id = documentId,
            Content = Encoding.UTF8.GetBytes("Document content"),
            ContentType = "application/pdf",
            FileName = "document.pdf"
        });
    }

    public Task<DocumentDto?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue(documentId, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<DocumentDto>> ListAsync(ListDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        var docs = _documents.Values
            .Where(d => d.PartyId == request.PartyId)
            .Where(d => request.DocumentType == null || d.DocumentType == request.DocumentType)
            .Where(d => request.RelatedEntityId == null || d.RelatedEntityId == request.RelatedEntityId)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();
        return Task.FromResult<IReadOnlyList<DocumentDto>>(docs);
    }

    public Task DeleteAsync(Guid documentId, string deletedBy, CancellationToken cancellationToken = default)
    {
        if (_documents.TryGetValue(documentId, out var doc))
            _documents[documentId] = doc with { IsDeleted = true };
        return Task.CompletedTask;
    }
}

public class BudgetingService : IBudgetingService
{
    private readonly ConcurrentDictionary<Guid, BudgetDto> _budgets = new();

    public Task CategorizeTransactionAsync(CategorizeTransactionRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<TransactionCategoryDto?> AutoCategorizeAsync(Guid transactionId, string description, string? merchantName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TransactionCategoryDto?>(new TransactionCategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Uncategorized"
        });
    }

    public Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        var budget = new BudgetDto
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            CategoryId = request.CategoryId,
            CurrencyCode = request.CurrencyCode,
            AmountLimitMinorUnits = request.AmountLimitMinorUnits,
            AmountLimitDecimal = request.AmountLimitMinorUnits / 100m,
            Period = request.Period,
            StartDate = request.StartDate,
            EndDate = request.EndDate ?? request.StartDate.AddMonths(1),
            SpentAmountMinorUnits = 0,
            SpentAmountDecimal = 0,
            PercentageUsed = 0,
            IsAlertEnabled = request.IsAlertEnabled,
            AlertThresholdPercentage = request.AlertThresholdPercentage
        };
        _budgets[budget.Id] = budget;
        return Task.FromResult(budget);
    }

    public Task<BudgetDto?> GetBudgetAsync(Guid budgetId, CancellationToken cancellationToken = default)
    {
        _budgets.TryGetValue(budgetId, out var budget);
        return Task.FromResult(budget);
    }

    public Task<IReadOnlyList<BudgetDto>> GetBudgetsByPartyAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var budgets = _budgets.Values.Where(b => b.PartyId == partyId).ToList();
        return Task.FromResult<IReadOnlyList<BudgetDto>>(budgets);
    }

    public Task UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteBudgetAsync(Guid budgetId, CancellationToken cancellationToken = default)
    {
        _budgets.TryRemove(budgetId, out _);
        return Task.CompletedTask;
    }

    public Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(Guid partyId, string currencyCode, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SpendingAnalyticsDto
        {
            PartyId = partyId,
            CurrencyCode = currencyCode,
            FromDate = fromDate,
            ToDate = toDate,
            TotalSpentMinorUnits = 500000,
            TotalSpentDecimal = 5000m,
            ByCategory = new List<CategorySpendingDto>(),
            ByDay = new List<DailySpendingDto>()
        });
    }

    public Task<IReadOnlyList<SpendingTrendDto>> GetSpendingTrendsAsync(Guid partyId, string currencyCode, string period, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<SpendingTrendDto>>(new List<SpendingTrendDto>());
    }

    public Task CheckBudgetAlertsAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class AuditService : IAuditService
{
    private readonly ConcurrentDictionary<Guid, AuditEntryDto> _entries = new();

    public Task LogAsync(LogAuditRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new AuditEntryDto
        {
            Id = Guid.NewGuid(),
            Action = request.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            ActorType = request.ActorType,
            ActorId = request.ActorId,
            ActorIpAddress = request.ActorIpAddress,
            Timestamp = DateTime.UtcNow,
            BeforeState = request.BeforeState?.ToString(),
            AfterState = request.AfterState?.ToString(),
            Metadata = request.Metadata != null ? string.Join(";", request.Metadata.Select(kv => $"{kv.Key}={kv.Value}")) : null
        };
        _entries[entry.Id] = entry;
        return Task.CompletedTask;
    }

    public Task<AuditQueryResponse> QueryAsync(AuditQueryRequest request, CancellationToken cancellationToken = default)
    {
        var entries = _entries.Values
            .Where(e => request.ActorId == null || e.ActorId == request.ActorId)
            .Where(e => request.EntityType == null || e.EntityType == request.EntityType)
            .Where(e => request.EntityId == null || e.EntityId == request.EntityId)
            .Where(e => request.Action == null || e.Action == request.Action)
            .Where(e => request.FromDate == null || e.Timestamp >= request.FromDate)
            .Where(e => request.ToDate == null || e.Timestamp <= request.ToDate)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Task.FromResult(new AuditQueryResponse
        {
            Entries = entries,
            TotalCount = entries.Count
        });
    }
}

public class SchedulerService : ISchedulerService
{
    private readonly ConcurrentDictionary<string, ScheduledJobDto> _jobs = new();

    public SchedulerService()
    {
        _jobs["reconciliation"] = new ScheduledJobDto
        {
            JobId = "reconciliation",
            JobName = "Daily Reconciliation",
            Description = "Reconcile ledger with accounts",
            CronExpression = "0 2 * * *",
            IsActive = true
        };
        _jobs["statements"] = new ScheduledJobDto
        {
            JobId = "statements",
            JobName = "Monthly Statements",
            Description = "Generate monthly statements",
            CronExpression = "0 3 1 * *",
            IsActive = true
        };
    }

    public Task<IReadOnlyList<ScheduledJobDto>> GetScheduledJobsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ScheduledJobDto>>(_jobs.Values.ToList());
    }

    public Task<ScheduledJobDto?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task TriggerJobAsync(TriggerJobRequest request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Triggering job: {request.JobId}");
        return Task.CompletedTask;
    }

    public Task PauseJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (_jobs.TryGetValue(jobId, out var job))
            _jobs[jobId] = job with { IsActive = false };
        return Task.CompletedTask;
    }

    public Task ResumeJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (_jobs.TryGetValue(jobId, out var job))
            _jobs[jobId] = job with { IsActive = true };
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<JobExecutionHistoryDto>> GetJobHistoryAsync(string jobId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<JobExecutionHistoryDto>>(new List<JobExecutionHistoryDto>());
    }
}
