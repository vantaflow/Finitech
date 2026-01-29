using Finitech.Modules.Banking.Contracts;
using Finitech.Modules.Banking.Contracts.DTOs;
using Finitech.Modules.IdentityAccess.Contracts;
using Finitech.Modules.IdentityAccess.Contracts.DTOs;
using Finitech.Modules.IdentityCompliance.Contracts;
using Finitech.Modules.IdentityCompliance.Contracts.DTOs;
using Finitech.Modules.MerchantPayments.Contracts;
using Finitech.Modules.MerchantPayments.Contracts.DTOs;
using Finitech.Modules.PartyRegistry.Contracts;
using Finitech.Modules.PartyRegistry.Contracts.DTOs;
using Finitech.Modules.Payments.Contracts;
using Finitech.Modules.Payments.Contracts.DTOs;
using Finitech.Modules.Wallet.Contracts;
using Finitech.Modules.Wallet.Contracts.DTOs;
using Finitech.Modules.WalletFMCG.Contracts;
using Finitech.Modules.WalletFMCG.Contracts.DTOs;
using Finitech.Modules.BranchNetwork.Contracts;
using Finitech.Modules.BranchNetwork.Contracts.DTOs;
using Finitech.Modules.Notifications.Contracts;
using Finitech.Modules.Ledger.Contracts;
using AddressDto = Finitech.Modules.BranchNetwork.Contracts.DTOs.AddressDto;

namespace Finitech.ApiHost.Services;

public class SeedDataService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SeedDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var partyRegistry = scope.ServiceProvider.GetRequiredService<IPartyRegistryService>();
        var identityAccess = scope.ServiceProvider.GetRequiredService<IIdentityAccessService>();
        var compliance = scope.ServiceProvider.GetRequiredService<IIdentityComplianceService>();
        var ledger = scope.ServiceProvider.GetRequiredService<ILedgerService>();
        var banking = scope.ServiceProvider.GetRequiredService<IBankingService>();
        var wallet = scope.ServiceProvider.GetRequiredService<IWalletService>();
        var walletFmcg = scope.ServiceProvider.GetRequiredService<IWalletFMCGService>();
        var merchantPayments = scope.ServiceProvider.GetRequiredService<IMerchantPaymentsService>();
        var payments = scope.ServiceProvider.GetRequiredService<IPaymentsService>();
        var branchNetwork = scope.ServiceProvider.GetRequiredService<IBranchNetworkService>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationsService>();

        // 1. Create parties with different roles
        var consumerParty = await partyRegistry.CreatePartyAsync(new CreatePartyRequest
        {
            PartyType = "Individual",
            FirstName = "Ahmed",
            LastName = "Benali",
            DisplayName = "Ahmed Benali",
            Email = "ahmed.benali@example.com",
            PhoneNumber = "+212612345678",
            InitialRoles = new List<string> { "Consumer", "RetailCustomer" }
        }, cancellationToken);

        var merchantParty = await partyRegistry.CreatePartyAsync(new CreatePartyRequest
        {
            PartyType = "Business",
            BusinessName = "Café Central",
            DisplayName = "Café Central",
            Email = "contact@cafe-central.ma",
            PhoneNumber = "+212612345679",
            InitialRoles = new List<string> { "Merchant" }
        }, cancellationToken);

        var agentParty = await partyRegistry.CreatePartyAsync(new CreatePartyRequest
        {
            PartyType = "Individual",
            FirstName = "Omar",
            LastName = "El Amrani",
            DisplayName = "Omar El Amrani",
            Email = "omar.agent@example.com",
            PhoneNumber = "+212612345680",
            InitialRoles = new List<string> { "RetailAgent" }
        }, cancellationToken);

        var distributorParty = await partyRegistry.CreatePartyAsync(new CreatePartyRequest
        {
            PartyType = "Business",
            BusinessName = "Distrib Express",
            DisplayName = "Distrib Express",
            Email = "contact@distribexpress.ma",
            PhoneNumber = "+212612345681",
            InitialRoles = new List<string> { "Distributor" }
        }, cancellationToken);

        var proParty = await partyRegistry.CreatePartyAsync(new CreatePartyRequest
        {
            PartyType = "Business",
            BusinessName = "Tech Solutions SARL",
            DisplayName = "Tech Solutions SARL",
            Email = "contact@techsolutions.ma",
            PhoneNumber = "+212612345682",
            InitialRoles = new List<string> { "ProCustomer" }
        }, cancellationToken);

        // 2. Create users
        await identityAccess.RegisterAsync(new RegisterRequest
        {
            Email = "ahmed.benali@example.com",
            PhoneNumber = "+212612345678",
            Password = "Password123!",
            PartyId = consumerParty.Id
        }, cancellationToken);

        await identityAccess.RegisterAsync(new RegisterRequest
        {
            Email = "contact@cafe-central.ma",
            PhoneNumber = "+212612345679",
            Password = "Password123!",
            PartyId = merchantParty.Id
        }, cancellationToken);

        // 3. Create KYC/KYB
        await compliance.SubmitKYCAsync(new SubmitKYCRequest
        {
            PartyId = consumerParty.Id,
            DocumentType = "NationalId",
            DocumentNumber = "AB123456",
            DocumentExpiryDate = DateTime.UtcNow.AddYears(5),
            DocumentFrontImageUrl = "https://storage.example.com/kyc/front1.jpg",
            DocumentBackImageUrl = "https://storage.example.com/kyc/back1.jpg",
            SelfieImageUrl = "https://storage.example.com/kyc/selfie1.jpg"
        }, cancellationToken);

        var kyc = await compliance.GetKYCStatusAsync(consumerParty.Id, cancellationToken);
        if (kyc != null)
        {
            await compliance.ReviewKYCAsync(kyc.Id, new ReviewKYCRequest
            {
                Decision = "Approved",
                ReviewedBy = "system"
            }, cancellationToken);
        }

        // 4. Create bank accounts
        var currentAccount = await banking.CreateAccountAsync(new CreateBankAccountRequest
        {
            PartyId = consumerParty.Id,
            AccountType = "Current",
            CurrencyCode = "MAD",
            AccountNumber = "007123456789012345678901"
        }, cancellationToken);

        var eurAccount = await banking.CreateAccountAsync(new CreateBankAccountRequest
        {
            PartyId = consumerParty.Id,
            AccountType = "Current",
            CurrencyCode = "EUR",
            AccountNumber = "007123456789012345678902"
        }, cancellationToken);

        var savingsAccount = await banking.CreateSavingsAccountAsync(new CreateSavingsAccountRequest
        {
            PartyId = consumerParty.Id,
            CurrencyCode = "MAD",
            InterestRate = 0.025m,
            MinimumBalanceMinorUnits = 100000,
            InitialDepositMinorUnits = 500000
        }, cancellationToken);

        // 5. Create wallets
        var walletAccount = await wallet.CreateWalletAsync(new CreateWalletRequest
        {
            PartyId = consumerParty.Id,
            InitialLevel = "Standard",
            SupportedCurrencies = new List<string> { "MAD", "EUR", "USD" }
        }, cancellationToken);

        // 6. Seed ledger balances
        var ledgerService = (LedgerService)ledger;
        ledgerService.SeedBalance(currentAccount.Id, "MAD", 1000000); // 10,000 MAD
        ledgerService.SeedBalance(eurAccount.Id, "EUR", 50000);       // 500 EUR
        ledgerService.SeedBalance(walletAccount.Id, "MAD", 500000);          // 5,000 MAD
        ledgerService.SeedBalance(walletAccount.Id, "EUR", 20000);           // 200 EUR
        ledgerService.SeedBalance(walletAccount.Id, "USD", 10000);           // 100 USD

        // 7. Create merchant
        await merchantPayments.CreateMerchantAsync(new CreateMerchantRequest
        {
            PartyId = merchantParty.Id,
            BusinessName = "Café Central",
            Category = "Food & Beverage",
            MerchantCode = "MERCH001"
        }, cancellationToken);

        // 8. Create float accounts
        await walletFmcg.CreateFloatAccountAsync(agentParty.Id, "RetailAgent", cancellationToken);
        await walletFmcg.CreateFloatAccountAsync(distributorParty.Id, "Distributor", cancellationToken);

        // 9. Create branches
        await branchNetwork.CreateBranchAsync(new CreateBranchRequest
        {
            Name = "Agence Centrale",
            Code = "BR001",
            Address = new AddressDto
            {
                Street = "123 Avenue Mohammed V",
                City = "Casablanca",
                PostalCode = "20000",
                Country = "Maroc"
            },
            Latitude = 33.5731,
            Longitude = -7.5898,
            PhoneNumber = "+212522123456",
            Email = "agence.centrale@finitech.ma",
            Services = new List<string> { "AccountOpening", "Loans", "Cards", "Exchange" },
            WorkingHours = new WorkingHoursDto
            {
                Monday = new WorkingDayDto { IsOpen = true, OpenTime = TimeSpan.FromHours(8), CloseTime = TimeSpan.FromHours(16.5) },
                Tuesday = new WorkingDayDto { IsOpen = true, OpenTime = TimeSpan.FromHours(8), CloseTime = TimeSpan.FromHours(16.5) },
                Wednesday = new WorkingDayDto { IsOpen = true, OpenTime = TimeSpan.FromHours(8), CloseTime = TimeSpan.FromHours(16.5) },
                Thursday = new WorkingDayDto { IsOpen = true, OpenTime = TimeSpan.FromHours(8), CloseTime = TimeSpan.FromHours(16.5) },
                Friday = new WorkingDayDto { IsOpen = true, OpenTime = TimeSpan.FromHours(8), CloseTime = TimeSpan.FromHours(12) },
                Saturday = new WorkingDayDto { IsOpen = false },
                Sunday = new WorkingDayDto { IsOpen = false }
            }
        }, cancellationToken);

        // 10. Create beneficiaries
        await payments.CreateBeneficiaryAsync(new CreateBeneficiaryRequest
        {
            OwnerPartyId = consumerParty.Id,
            Name = "Electricité du Maroc",
            BeneficiaryType = "BillPay",
            Identifier = "FACTURE_EDM"
        }, cancellationToken);

        Console.WriteLine($"Seed data completed successfully!");
        Console.WriteLine($"Created parties: Consumer={consumerParty.Id}, Merchant={merchantParty.Id}, Agent={agentParty.Id}, Distributor={distributorParty.Id}");
        Console.WriteLine($"Created accounts: Current={currentAccount.Id}, Savings={savingsAccount.Id}");
        Console.WriteLine($"Created wallet: {walletAccount.Id}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
