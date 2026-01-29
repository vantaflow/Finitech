# Finitech - FinTech Platform Architecture

Architecture .NET complète pour une plateforme FinTech couvrant Banking et Wallet avec séparation stricte des domaines.

## Architecture

### Choix Architectural: Modular Monolith

Nous avons choisi l'approche **Modular Monolith** pour les raisons suivantes:
- **Cohérence transactionnelle forte**: Le ledger immuable nécessite des transactions ACID strictes
- **Performance à fort volume**: Éviter la latence réseau des appels inter-services en production
- **Simplicité opérationnelle**: Un seul déploiement, une seule base de données, monitoring simplifié
- **Migration progressive**: Facilement décomposable en microservices si nécessaire plus tard
- **Consistance des données**: Garantie par la base de données relationnelle

### Structure des Modules

```
/src
  /BuildingBlocks
    /Finitech.BuildingBlocks.Domain        # Interfaces, repositories, Result
    /Finitech.BuildingBlocks.Application   # CQRS, MediatR patterns
    /Finitech.BuildingBlocks.Infrastructure # EF Core, messaging
    /Finitech.BuildingBlocks.Contracts     # DTOs partagés
    /Finitech.BuildingBlocks.SharedKernel  # Money, ValueObjects, Entities

  /Modules
    /PartyRegistry        # Référentiel commun clients/parties
    /IdentityAccess       # Authentification, login, reset password
    /IdentityCompliance   # eKYC, KYB, AML, fraude
    /BranchNetwork        # Gestion des agences
    /Ledger               # Source de vérité money-movement
    /FX                   # Taux de change, conversions
    /Payments             # Virements, factures, ordres permanents
    /Statements           # Relevés comptables
    /MerchantPayments     # QR EMVCo, paiement marchand
    /Disputes             # Refunds, chargebacks
    /Notifications        # Email, SMS, Push
    /Documents            # Stockage documents
    /Budgeting            # Catégorisation, budgets
    /Audit                # Audit trail compliance
    /Scheduler            # Jobs planifiés
    /Wallet               # Portefeuille digital (P2P, loyalty)
    /WalletFMCG           # Distribution, agents, commissions
    /Banking              # Comptes bancaires, prêts, cartes

  /ApiHost              # Host ASP.NET Core
/tests
  /UnitTests
  /IntegrationTests
  /ArchitectureTests    # NetArchTest.Rules
```

### Règles de Dépendances (OBLIGATOIRES)

```
┌─────────────────────────────────────────────────────────────────┐
│                        API HOST                                 │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────────┐  ┌─────────────┐  │
│  │ Banking  │  │  Wallet  │  │ WalletFMCG   │  │   Ledger    │  │
│  │ (Spécif) │  │ (Spécif) │  │  (Spécif)    │  │   (Commun)  │  │
│  └────┬─────┘  └────┬─────┘  └──────┬───────┘  └──────┬──────┘  │
│       │             │                │                │         │
│       └─────────────┴────────────────┴────────────────┘         │
│                          │                                      │
│       ┌──────────────────┴──────────────────┐                   │
│       │                                     │                   │
│  ┌────┴─────┐  ┌──────────┐  ┌──────────┐   │                   │
│  │Payments  │  │   FX     │  │Statements│   │                   │
│  │(Commun)  │  │(Commun)  │  │(Commun)  │   │                   │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘   │                   │
│       │             │             │         │                   │
│       └─────────────┴─────────────┘         │                   │
│                     │                       │                   │
│              ┌──────┴──────┐                │                   │
│              │             │                │                   │
│        ┌─────┴────┐   ┌────┴────┐     ┌─────┴─────┐             │
│        │PartyReg. │   │Identity │     │  Branch   │             │
│        │(Shared)  │   │(Shared) │     │  Network  │             │
│        └──────────┘   └─────────┘     └───────────┘             │
│                                                                 │
│        ┌─────────────────────────────────────────┐              │
│        │         BuildingBlocks                  │              │
│        │  (Domain, Application, Infrastructure)  │              │
│        └─────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────────┘
```

**Règles STRICTES:**
- ❌ Banking ne peut JAMAIS référencer Wallet ou WalletFMCG
- ❌ Wallet ne peut JAMAIS référencer Banking
- ✅ Les deux peuvent référencer les modules communs (Ledger, Payments, FX, PartyRegistry)
- ✅ La communication inter-modules se fait uniquement via Contracts + interfaces

## Stack Technique

- **.NET 8.0**
- **SQL Server 2022** (Docker pour dev)
- **Entity Framework Core** + Migrations
- **JWT Authentication**
- **OpenAPI/Swagger**
- **xUnit** pour les tests

## Démarrage Rapide

### Prérequis
- Docker Desktop
- .NET 8 SDK
- cURL ou Postman

### 1. Démarrer SQL Server

```bash
docker-compose up -d sqlserver
```

Attendre que SQL Server soit prêt (~30s):
```bash
docker logs -f finitech-sqlserver
```

### 2. Lancer l'application

```bash
dotnet run --project src/ApiHost/Finitech.ApiHost/Finitech.ApiHost.csproj
```

L'API sera disponible sur: `https://localhost:5001` ou `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

### 3. Exécuter avec Docker Compose (tout en un)

```bash
docker-compose up --build
```

## Exemples cURL

### PartyRegistry - Créer un Party (Consumer + RetailCustomer)

```bash
curl -X POST http://localhost:5000/api/partyregistry \
  -H "Content-Type: application/json" \
  -d '{
    "partyType": "Individual",
    "firstName": "Ahmed",
    "lastName": "Benali",
    "displayName": "Ahmed Benali",
    "email": "ahmed.benali@example.com",
    "phoneNumber": "+212612345678",
    "initialRoles": ["Consumer", "RetailCustomer"]
  }'
```

### PartyRegistry - Assigner un rôle Merchant

```bash
curl -X POST http://localhost:5000/api/partyregistry/{partyId}/roles \
  -H "Content-Type: application/json" \
  -d '{
    "role": "Merchant",
    "domain": "Wallet"
  }'
```

### Compliance - Soumettre eKYC

```bash
curl -X POST http://localhost:5000/api/compliance/kyc \
  -H "Content-Type: application/json" \
  -d '{
    "partyId": "{partyId}",
    "documentType": "NationalId",
    "documentNumber": "AB123456",
    "documentExpiryDate": "2029-12-31T00:00:00Z",
    "documentFrontImageUrl": "https://storage.example.com/front.jpg",
    "documentBackImageUrl": "https://storage.example.com/back.jpg",
    "selfieImageUrl": "https://storage.example.com/selfie.jpg"
  }'
```

### Compliance - Approuver KYC

```bash
curl -X POST http://localhost:5000/api/compliance/kyc/{kycId}/review \
  -H "Content-Type: application/json" \
  -d '{
    "decision": "Approved",
    "reviewedBy": "admin001"
  }'
```

### Ledger - Consulter les balances multi-devise

```bash
curl http://localhost:5000/api/ledger/accounts/{accountId}/balances
```

**Réponse:**
```json
{
  "accountId": "...",
  "balances": [
    { "currencyCode": "MAD", "amountMinorUnits": 1000000, "amountDecimal": 10000.00, "currencyNumericCode": 504 },
    { "currencyCode": "EUR", "amountMinorUnits": 50000, "amountDecimal": 500.00, "currencyNumericCode": 978 },
    { "currencyCode": "USD", "amountMinorUnits": 10000, "amountDecimal": 100.00, "currencyNumericCode": 840 }
  ]
}
```

### Ledger - Historique des écritures

```bash
curl -X POST http://localhost:5000/api/ledger/accounts/{accountId}/history \
  -H "Content-Type: application/json" \
  -d '{
    "currencyCode": "MAD",
    "fromDate": "2024-01-01T00:00:00Z",
    "toDate": "2024-12-31T23:59:59Z",
    "skip": 0,
    "take": 50
  }'
```

### FX - Obtenir un taux

```bash
curl -X POST http://localhost:5000/api/fx/rate \
  -H "Content-Type: application/json" \
  -d '{
    "fromCurrencyCode": "MAD",
    "toCurrencyCode": "EUR"
  }'
```

### FX - Créer une quote de conversion

```bash
curl -X POST http://localhost:5000/api/fx/quote \
  -H "Content-Type: application/json" \
  -d '{
    "fromCurrencyCode": "MAD",
    "toCurrencyCode": "EUR",
    "amountMinorUnits": 100000
  }'
```

### FX - Exécuter la conversion

```bash
curl -X POST http://localhost:5000/api/fx/convert \
  -H "Content-Type: application/json" \
  -d '{
    "quoteId": "{quoteId}",
    "sourceAccountId": "{walletId}",
    "targetAccountId": "{bankAccountId}",
    "idempotencyKey": "conv-001"
  }'
```

### Payments - Virement intra-devise

```bash
curl -X POST http://localhost:5000/api/payments/transfer \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccountId": "{accountId}",
    "toAccountId": "{beneficiaryAccountId}",
    "currencyCode": "MAD",
    "amountMinorUnits": 50000,
    "description": "Paiement facture",
    "idempotencyKey": "transfer-001"
  }'
```

### Payments - Virement cross-devise

```bash
curl -X POST http://localhost:5000/api/payments/cross-currency-transfer \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccountId": "{madAccountId}",
    "toAccountId": "{eurAccountId}",
    "fromCurrencyCode": "MAD",
    "toCurrencyCode": "EUR",
    "amountMinorUnits": 100000,
    "idempotencyKey": "fx-transfer-001"
  }'
```

### Payments - Ordre permanent (Standing Order)

```bash
curl -X POST http://localhost:5000/api/payments/standing-orders \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccountId": "{accountId}",
    "toAccountId": "{beneficiaryId}",
    "currencyCode": "MAD",
    "amountMinorUnits": 50000,
    "frequency": "Monthly",
    "startDate": "2024-02-01T00:00:00Z",
    "endDate": "2024-12-31T00:00:00Z",
    "description": "Loyer mensuel"
  }'
```

### Banking - Créer un compte épargne

```bash
curl -X POST http://localhost:5000/api/banking/savings \
  -H "Content-Type: application/json" \
  -d '{
    "partyId": "{partyId}",
    "currencyCode": "MAD",
    "interestRate": 0.025,
    "minimumBalanceMinorUnits": 100000,
    "initialDepositMinorUnits": 500000
  }'
```

### Banking - Calculer les intérêts

```bash
curl -X POST http://localhost:5000/api/banking/accounts/{savingsAccountId}/calculate-interest
```

### Banking - Demande de prêt

```bash
curl -X POST http://localhost:5000/api/banking/loans \
  -H "Content-Type: application/json" \
  -d '{
    "partyId": "{partyId}",
    "requestedAmountMinorUnits": 5000000,
    "requestedDurationMonths": 24,
    "purpose": "Achat équipement",
    "employmentStatus": "Employed",
    "monthlyIncomeMinorUnits": 1500000
  }'
```

### Banking - Approuver un prêt

```bash
curl -X POST http://localhost:5000/api/banking/loans/{loanId}/approve \
  -H "Content-Type: application/json" \
  -d '{
    "loanId": "{loanId}",
    "approved": true,
    "approvedInterestRate": 0.055,
    "approvedBy": "manager001"
  }'
```

### Banking - Découvert autorisé

```bash
curl -X POST http://localhost:5000/api/banking/accounts/{accountId}/overdraft \
  -H "Content-Type: application/json" \
  -d '{
    "limitMinorUnits": 100000,
    "interestRate": 0.08
  }'
```

### Wallet - P2P Send

```bash
curl -X POST http://localhost:5000/api/wallet/p2p/send \
  -H "Content-Type: application/json" \
  -d '{
    "fromWalletId": "{walletId}",
    "toIdentifier": "+212612345679",
    "identifierType": "Phone",
    "currencyCode": "MAD",
    "amountMinorUnits": 25000,
    "description": "Remboursement déjeuner",
    "idempotencyKey": "p2p-001"
  }'
```

### Wallet - Split Payment

```bash
curl -X POST http://localhost:5000/api/wallet/split \
  -H "Content-Type: application/json" \
  -d '{
    "initiatorWalletId": "{walletId}",
    "participantIdentifiers": ["+212612345679", "+212612345680"],
    "currencyCode": "MAD",
    "totalAmountMinorUnits": 90000,
    "description": "Dîner entre amis"
  }'
```

### Wallet - Redeem Loyalty Points

```bash
curl -X POST http://localhost:5000/api/wallet/{walletId}/loyalty/redeem \
  -H "Content-Type: application/json" \
  -d '500'
```

### WalletFMCG - Cash-In

```bash
curl -X POST http://localhost:5000/api/walletfmcg/cash-in \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "{agentId}",
    "currencyCode": "MAD",
    "amountMinorUnits": 50000,
    "customerWalletId": "{customerWalletId}",
    "reference": "CI-001",
    "idempotencyKey": "cashin-001"
  }'
```

### WalletFMCG - Calculer commission

```bash
curl -X POST http://localhost:5000/api/walletfmcg/commissions/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "beneficiaryId": "{agentId}",
    "beneficiaryType": "Agent",
    "operationType": "CashIn",
    "currencyCode": "MAD",
    "amountMinorUnits": 50000,
    "originalTransactionId": "{transactionId}"
  }'
```

### MerchantPayments - Générer QR EMVCo Dynamique

```bash
curl -X POST http://localhost:5000/api/merchantpayments/qr/generate \
  -H "Content-Type: application/json" \
  -d '{
    "merchantId": "{merchantId}",
    "currencyCode": "MAD",
    "amountMinorUnits": 15000,
    "reference": "CMD-001",
    "description": "Café + Croissant",
    "expiresAt": "2024-01-15T14:30:00Z"
  }'
```

**Réponse (Payload EMVCo):**
```json
{
  "payload": "000201010212150412345104567853035045401015.006002CASABLANCA62400505CMD-0016304A1B2",
  "payloadFormat": "EMVCo",
  "payloadLength": 87,
  "currencyNumericCode": "504",
  "amount": 150.00,
  "reference": "CMD-001",
  "crc": "A1B2"
}
```

### MerchantPayments - Payer par QR

```bash
curl -X POST http://localhost:5000/api/merchantpayments/qr/pay \
  -H "Content-Type: application/json" \
  -d '{
    "qrPayload": "000201010212150412345104567853035045401015.006002CASABLANCA62400505CMD-0016304A1B2",
    "payerWalletId": "{walletId}",
    "idempotencyKey": "qrpay-001"
  }'
```

### Disputes - Remboursement partiel

```bash
curl -X POST http://localhost:5000/api/disputes/refund \
  -H "Content-Type: application/json" \
  -d '{
    "originalTransactionId": "{transactionId}",
    "amountMinorUnits": 5000,
    "reason": "Produit défectueux",
    "idempotencyKey": "refund-001"
  }'
```

### Disputes - Initier un chargeback

```bash
curl -X POST http://localhost:5000/api/disputes/chargeback \
  -H "Content-Type: application/json" \
  -d '{
    "originalTransactionId": "{transactionId}",
    "reason": "Transaction non autorisée",
    "evidenceDescription": "Client indique ne pas avoir effectué cette transaction"
  }'
```

### Compliance - Action forte (Freeze Party)

```bash
curl -X POST http://localhost:5000/api/compliance/strong-actions \
  -H "Content-Type: application/json" \
  -d '{
    "actionType": "FreezeParty",
    "partyId": "{partyId}",
    "reason": "Suspicion de fraude détectée par le système",
    "initiatedBy": "fraud-system"
  }'
```

### Notifications - Envoyer une notification

```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "recipientPartyId": "{partyId}",
    "notificationType": "Transaction",
    "channel": "SMS",
    "subject": "Confirmation de transaction",
    "body": "Vous avez reçu 500 MAD de Ahmed Benali",
    "data": { "amount": "500", "currency": "MAD", "sender": "Ahmed Benali" }
  }'
```

### Documents - Upload

```bash
curl -X POST http://localhost:5000/api/documents/upload \
  -H "Content-Type: multipart/form-data" \
  -F "partyId={partyId}" \
  -F "documentType=KYC" \
  -F "file=@/path/to/document.pdf"
```

### Budgeting - Définir un budget

```bash
curl -X POST http://localhost:5000/api/budgeting/budgets \
  -H "Content-Type: application/json" \
  -d '{
    "partyId": "{partyId}",
    "categoryId": "restaurants",
    "currencyCode": "MAD",
    "amountLimitMinorUnits": 200000,
    "period": "Monthly",
    "startDate": "2024-01-01T00:00:00Z",
    "isAlertEnabled": true,
    "alertThresholdPercentage": 80
  }'
```

### Budgeting - Analytics de dépenses

```bash
curl "http://localhost:5000/api/budgeting/analytics?partyId={partyId}&currencyCode=MAD&fromDate=2024-01-01&toDate=2024-01-31"
```

## Tests

### Exécuter les tests unitaires

```bash
dotnet test tests/Finitech.UnitTests
```

### Exécuter les tests d'architecture

```bash
dotnet test tests/Finitech.ArchitectureTests
```

Les tests d'architecture vérifient:
- ❌ Que Banking ne référence pas Wallet
- ❌ Que Wallet ne référence pas Banking
- ✅ Que les modules respectent les dépendances vers Contracts uniquement
- ✅ Que le Domain ne dépend pas de l'Infrastructure

## Configuration

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Finitech;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long-for-jwt-tokens",
    "Issuer": "Finitech",
    "Audience": "FinitechApi",
    "ExpiryHours": 8
  }
}
```

## Multi-Devise

Le système supporte MAD, EUR, USD dès la V1:

- **Stockage**: Minor units (long) + CurrencyCode (string)
- **Conversion**: Via module FX avec taux simulés
- **Ledger**: Une écriture par devise, pas de conversion implicite
- **EMVCo QR**: Devise numérique ISO 4217 (504=MAD, 978=EUR, 840=USD)

## Hypothèses et Simplifications

1. **Authentification**: JWT simplifié (pas de refresh token rotation)
2. **Stockage**: In-memory pour la démo (à remplacer par EF Core + SQL Server)
3. **FX Rates**: Taux fixes simulés (à remplacer par fournisseur externe)
4. **Notifications**: Console output (à remplacer par fournisseurs SMS/Email)
5. **EMVCo QR**: Payload simplifié (pas de CRC réel)
6. **Idempotence**: Clé en mémoire (à remplacer par table dédiée)

## Roadmap Production

1. Implémenter EF Core DbContext par module
2. Ajouter messaging (RabbitMQ/Azure Service Bus) pour outbox
3. Implémenter vrais providers KYC/KYB (Jumio, Onfido)
4. Intégrer fournisseur FX (XE, Fixer)
5. Ajouter fournisseurs SMS/Email (Twilio, SendGrid)
6. Implémenter event sourcing pour le Ledger
7. Ajouter monitoring (Prometheus, Grafana)
8. Déployer sur Kubernetes

## Licence

MIT - Ce projet est un exemple éducatif d'architecture .NET.
