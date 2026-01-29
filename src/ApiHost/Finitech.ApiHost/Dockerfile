FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Finitech.sln", "./"]
COPY ["src/BuildingBlocks/Finitech.BuildingBlocks.Domain/Finitech.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/Finitech.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/Finitech.BuildingBlocks.Application/Finitech.BuildingBlocks.Application.csproj", "src/BuildingBlocks/Finitech.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/Finitech.BuildingBlocks.Infrastructure/Finitech.BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/Finitech.BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/Finitech.BuildingBlocks.Contracts/Finitech.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/Finitech.BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/Finitech.BuildingBlocks.SharedKernel/Finitech.BuildingBlocks.SharedKernel.csproj", "src/BuildingBlocks/Finitech.BuildingBlocks.SharedKernel/"]
COPY ["src/ApiHost/Finitech.ApiHost/Finitech.ApiHost.csproj", "src/ApiHost/Finitech.ApiHost/"]

# Module projects - PartyRegistry
COPY ["src/Modules/PartyRegistry/Domain/Finitech.Modules.PartyRegistry.Domain.csproj", "src/Modules/PartyRegistry/Domain/"]
COPY ["src/Modules/PartyRegistry/Application/Finitech.Modules.PartyRegistry.Application.csproj", "src/Modules/PartyRegistry/Application/"]
COPY ["src/Modules/PartyRegistry/Infrastructure/Finitech.Modules.PartyRegistry.Infrastructure.csproj", "src/Modules/PartyRegistry/Infrastructure/"]
COPY ["src/Modules/PartyRegistry/Contracts/Finitech.Modules.PartyRegistry.Contracts.csproj", "src/Modules/PartyRegistry/Contracts/"]

# Ledger module
COPY ["src/Modules/Ledger/Domain/Finitech.Modules.Ledger.Domain.csproj", "src/Modules/Ledger/Domain/"]
COPY ["src/Modules/Ledger/Infrastructure/Finitech.Modules.Ledger.Infrastructure.csproj", "src/Modules/Ledger/Infrastructure/"]
COPY ["src/Modules/Ledger/Contracts/Finitech.Modules.Ledger.Contracts.csproj", "src/Modules/Ledger/Contracts/"]

# FX module
COPY ["src/Modules/FX/Domain/Finitech.Modules.FX.Domain.csproj", "src/Modules/FX/Domain/"]
COPY ["src/Modules/FX/Contracts/Finitech.Modules.FX.Contracts.csproj", "src/Modules/FX/Contracts/"]

# Wallet module
COPY ["src/Modules/Wallet/Domain/Finitech.Modules.Wallet.Domain.csproj", "src/Modules/Wallet/Domain/"]
COPY ["src/Modules/Wallet/Contracts/Finitech.Modules.Wallet.Contracts.csproj", "src/Modules/Wallet/Contracts/"]

# WalletFMCG module
COPY ["src/Modules/WalletFMCG/Domain/Finitech.Modules.WalletFMCG.Domain.csproj", "src/Modules/WalletFMCG/Domain/"]
COPY ["src/Modules/WalletFMCG/Contracts/Finitech.Modules.WalletFMCG.Contracts.csproj", "src/Modules/WalletFMCG/Contracts/"]

# Banking module
COPY ["src/Modules/Banking/Domain/Finitech.Modules.Banking.Domain.csproj", "src/Modules/Banking/Domain/"]
COPY ["src/Modules/Banking/Contracts/Finitech.Modules.Banking.Contracts.csproj", "src/Modules/Banking/Contracts/"]

# IdentityAccess - flat structure
COPY ["src/Modules/Finitech.Modules.IdentityAccess.Contracts/Finitech.Modules.IdentityAccess.Contracts.csproj", "src/Modules/Finitech.Modules.IdentityAccess.Contracts/"]
COPY ["src/Modules/Finitech.Modules.IdentityAccess.Domain/Finitech.Modules.IdentityAccess.Domain.csproj", "src/Modules/Finitech.Modules.IdentityAccess.Domain/"]
COPY ["src/Modules/Finitech.Modules.IdentityAccess.Application/Finitech.Modules.IdentityAccess.Application.csproj", "src/Modules/Finitech.Modules.IdentityAccess.Application/"]
COPY ["src/Modules/Finitech.Modules.IdentityAccess.Infrastructure/Finitech.Modules.IdentityAccess.Infrastructure.csproj", "src/Modules/Finitech.Modules.IdentityAccess.Infrastructure/"]

# IdentityCompliance module
COPY ["src/Modules/IdentityCompliance/Domain/Finitech.Modules.IdentityCompliance.Domain.csproj", "src/Modules/IdentityCompliance/Domain/"]
COPY ["src/Modules/IdentityCompliance/Contracts/Finitech.Modules.IdentityCompliance.Contracts.csproj", "src/Modules/IdentityCompliance/Contracts/"]

# Payments module
COPY ["src/Modules/Payments/Domain/Finitech.Modules.Payments.Domain.csproj", "src/Modules/Payments/Domain/"]
COPY ["src/Modules/Payments/Contracts/Finitech.Modules.Payments.Contracts.csproj", "src/Modules/Payments/Contracts/"]

# MerchantPayments module
COPY ["src/Modules/MerchantPayments/Domain/Finitech.Modules.MerchantPayments.Domain.csproj", "src/Modules/MerchantPayments/Domain/"]
COPY ["src/Modules/MerchantPayments/Contracts/Finitech.Modules.MerchantPayments.Contracts.csproj", "src/Modules/MerchantPayments/Contracts/"]

# Disputes module
COPY ["src/Modules/Disputes/Domain/Finitech.Modules.Disputes.Domain.csproj", "src/Modules/Disputes/Domain/"]
COPY ["src/Modules/Disputes/Contracts/Finitech.Modules.Disputes.Contracts.csproj", "src/Modules/Disputes/Contracts/"]

# Notifications module
COPY ["src/Modules/Notifications/Domain/Finitech.Modules.Notifications.Domain.csproj", "src/Modules/Notifications/Domain/"]
COPY ["src/Modules/Notifications/Contracts/Finitech.Modules.Notifications.Contracts.csproj", "src/Modules/Notifications/Contracts/"]

# Documents module
COPY ["src/Modules/Documents/Domain/Finitech.Modules.Documents.Domain.csproj", "src/Modules/Documents/Domain/"]
COPY ["src/Modules/Documents/Contracts/Finitech.Modules.Documents.Contracts.csproj", "src/Modules/Documents/Contracts/"]

# Budgeting module
COPY ["src/Modules/Budgeting/Domain/Finitech.Modules.Budgeting.Domain.csproj", "src/Modules/Budgeting/Domain/"]
COPY ["src/Modules/Budgeting/Contracts/Finitech.Modules.Budgeting.Contracts.csproj", "src/Modules/Budgeting/Contracts/"]

# Audit module
COPY ["src/Modules/Audit/Domain/Finitech.Modules.Audit.Domain.csproj", "src/Modules/Audit/Domain/"]
COPY ["src/Modules/Audit/Contracts/Finitech.Modules.Audit.Contracts.csproj", "src/Modules/Audit/Contracts/"]

# Statements module
COPY ["src/Modules/Statements/Domain/Finitech.Modules.Statements.Domain.csproj", "src/Modules/Statements/Domain/"]
COPY ["src/Modules/Statements/Contracts/Finitech.Modules.Statements.Contracts.csproj", "src/Modules/Statements/Contracts/"]

# BranchNetwork module
COPY ["src/Modules/BranchNetwork/Domain/Finitech.Modules.BranchNetwork.Domain.csproj", "src/Modules/BranchNetwork/Domain/"]
COPY ["src/Modules/BranchNetwork/Contracts/Finitech.Modules.BranchNetwork.Contracts.csproj", "src/Modules/BranchNetwork/Contracts/"]

# Scheduler module
COPY ["src/Modules/Scheduler/Domain/Finitech.Modules.Scheduler.Domain.csproj", "src/Modules/Scheduler/Domain/"]
COPY ["src/Modules/Scheduler/Contracts/Finitech.Modules.Scheduler.Contracts.csproj", "src/Modules/Scheduler/Contracts/"]

RUN dotnet restore "src/ApiHost/Finitech.ApiHost/Finitech.ApiHost.csproj"

COPY . .
WORKDIR "/src/src/ApiHost/Finitech.ApiHost"
RUN dotnet build "Finitech.ApiHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Finitech.ApiHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Finitech.ApiHost.dll"]
