# Finitech - Manuel Utilisateur

## Plateforme FinTech Banking + Wallet

---

## Table des matières

1. [Introduction](#1-introduction)
2. [Premiers pas](#2-premiers-pas)
3. [Gestion des identités](#3-gestion-des-identités)
4. [Module Banking](#4-module-banking)
5. [Module Wallet](#5-module-wallet)
6. [Paiements](#6-paiements)
7. [Compliance & Sécurité](#7-compliance--sécurité)
8. [Rapports & Relevés](#8-rapports--relevés)
9. [FAQ & Dépannage](#9-faq--dépannage)

---

## 1. Introduction

### 1.1 Qu'est-ce que Finitech ?

Finitech est une plateforme FinTech complète qui combine :
- **Banking** : Comptes bancaires, épargne, prêts, cartes
- **Wallet** : Portefeuille digital, P2P, paiement marchand
- **Multi-devise** : MAD, EUR, USD
- **Compliance** : KYC/KYB, AML, détection fraude

### 1.2 Architecture

```
┌─────────────────────────────────────────┐
│           API Finitech                  │
├─────────────────────────────────────────┤
│  Banking  │  Wallet  │  WalletFMCG     │
├───────────┴──────────┴─────────────────┤
│  Ledger (Source de vérité)              │
│  FX (Taux de change)                    │
│  Payments (Virements)                   │
├─────────────────────────────────────────┤
│  Party Registry │ Identity & Compliance │
└─────────────────────────────────────────┘
```

### 1.3 Rôles utilisateurs

| Rôle | Description | Accès |
|------|-------------|-------|
| **Consumer** | Client particulier | Wallet, compte bancaire |
| **Merchant** | Commerçant | Encaissement QR, wallet |
| **RetailAgent** | Agent de proximité | Cash-in/Cash-out |
| **Distributor** | Distributeur | Gestion réseau agents |
| **ProCustomer** | Client professionnel | Services bancaires pro |

---

## 2. Premiers pas

### 2.1 Démarrage de l'application

#### Avec Docker (Recommandé)

```bash
# 1. Démarrer tous les services
docker-compose up -d

# 2. Vérifier que SQL Server est prêt
docker logs finitech-sqlserver

# 3. Accéder à l'API
# API : http://localhost:5000
# Swagger : http://localhost:5000/swagger
```

#### En local (Développement)

```bash
# 1. SQL Server doit être démarré
docker-compose up -d sqlserver

# 2. Lancer l'API
dotnet run --project src/ApiHost/Finitech.ApiHost
```

### 2.2 Configuration initiale

Le système crée automatiquement des données de test :
- **Particulier** : Ahmed Benali (ahmed.benali@example.com)
- **Marchand** : Café Central
- **Agent** : Omar El Amrani
- **Distributeur** : Distrib Express

---

## 3. Gestion des identités

### 3.1 Création d'un compte (Party)

#### Client Particulier
```bash
POST /api/partyregistry
{
  "partyType": "Individual",
  "firstName": "Fatima",
  "lastName": "Alami",
  "displayName": "Fatima Alami",
  "email": "fatima.alami@example.com",
  "phoneNumber": "+212612345683",
  "initialRoles": ["Consumer"]
}
```

#### Client Professionnel
```bash
POST /api/partyregistry
{
  "partyType": "Business",
  "businessName": "Ma Société SARL",
  "displayName": "Ma Société SARL",
  "email": "contact@masociete.ma",
  "phoneNumber": "+212512345678",
  "initialRoles": ["ProCustomer"]
}
```

### 3.2 Assignation de rôles

```bash
POST /api/partyregistry/{partyId}/roles
{
  "role": "Merchant",
  "domain": "Wallet"
}
```

Rôles disponibles :
- `Consumer` - Client wallet (particulier)
- `Merchant` - Commerçant
- `RetailAgent` - Agent de proximité
- `Distributor` - Distributeur
- `Institution` - Institution
- `RetailCustomer` - Client bancaire (particulier)
- `ProCustomer` - Client bancaire (pro)

### 3.3 Authentification

#### Inscription
```bash
POST /api/auth/register
{
  "email": "fatima.alami@example.com",
  "phoneNumber": "+212612345683",
  "password": "MonMotDePasse123!",
  "partyId": "{partyId}"
}
```

#### Connexion
```bash
POST /api/auth/login
{
  "emailOrPhone": "fatima.alami@example.com",
  "password": "MonMotDePasse123!",
  "deviceId": "device-001",
  "ipAddress": "192.168.1.1"
}
```

Réponse :
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
  "expiresAt": "2024-01-15T16:30:00Z",
  "userId": "...",
  "email": "fatima.alami@example.com"
}
```

#### Utilisation du token
```bash
curl -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  http://localhost:5000/api/wallet/...
```

### 3.4 Mot de passe oublié

```bash
# 1. Demander un reset
POST /api/auth/forgot-password
{
  "email": "fatima.alami@example.com"
}

# Réponse : { "resetToken": "abc123", "expiresAt": "..." }

# 2. Réinitialiser
POST /api/auth/reset-password
{
  "resetToken": "abc123",
  "newPassword": "NouveauMotDePasse456!"
}
```

---

## 4. Module Banking

### 4.1 Compte courant

#### Création
```bash
POST /api/banking/accounts
{
  "partyId": "{partyId}",
  "accountType": "Current",
  "currencyCode": "MAD",
  "accountNumber": null  // Auto-généré si null
}
```

#### Consulter les comptes
```bash
GET /api/banking/by-party/{partyId}/accounts
```

Réponse :
```json
[
  {
    "id": "...",
    "accountNumber": "007123456789012345678901",
    "accountType": "Current",
    "currencyCode": "MAD",
    "balanceMinorUnits": 1000000,
    "balanceDecimal": 10000.00,
    "status": "Active"
  }
]
```

### 4.2 Compte épargne

#### Création
```bash
POST /api/banking/savings
{
  "partyId": "{partyId}",
  "currencyCode": "MAD",
  "interestRate": 0.025,
  "minimumBalanceMinorUnits": 100000,
  "initialDepositMinorUnits": 500000
}
```

#### Déposer
```bash
POST /api/banking/accounts/{accountId}/deposit
50000  // amountMinorUnits
```

#### Retirer
```bash
POST /api/banking/accounts/{accountId}/withdraw
25000  // amountMinorUnits
```

#### Calcul des intérêts
```bash
POST /api/banking/accounts/{accountId}/calculate-interest
```

### 4.3 Dépôt à terme (Fixed Deposit)

#### Création
```bash
POST /api/banking/fixed-deposits
{
  "sourceAccountId": "{accountId}",
  "principalAmountMinorUnits": 1000000,
  "durationMonths": 12,
  "interestRate": 0.035,
  "autoRenewal": true
}
```

#### Suivi
```bash
GET /api/banking/fixed-deposits/{fixedDepositId}
```

Réponse :
```json
{
  "id": "...",
  "accountNumber": "FD1234567890",
  "principalAmountMinorUnits": 1000000,
  "maturityAmountMinorUnits": 1035000,
  "interestRate": 0.035,
  "durationMonths": 12,
  "maturityDate": "2025-01-15T00:00:00Z",
  "status": "Active"
}
```

### 4.4 Prêts

#### Demande
```bash
POST /api/banking/loans
{
  "partyId": "{partyId}",
  "requestedAmountMinorUnits": 5000000,
  "requestedDurationMonths": 24,
  "purpose": "Achat équipement",
  "employmentStatus": "Employed",
  "monthlyIncomeMinorUnits": 1500000
}
```

#### Suivi
```bash
GET /api/banking/loans/{loanId}
```

#### Remboursement
```bash
POST /api/banking/loans/{loanId}/repay?fromAccountId={accountId}
{
  "amountMinorUnits": 250000
}
```

### 4.5 Découvert autorisé

```bash
POST /api/banking/accounts/{accountId}/overdraft
{
  "limitMinorUnits": 100000,
  "interestRate": 0.08
}
```

### 4.6 Cartes bancaires

#### Gestion
```bash
# Geler la carte
POST /api/banking/cards/{cardId}/freeze

# Dégeler
POST /api/banking/cards/{cardId}/unfreeze

# Bloquer (perte/vol)
POST /api/banking/cards/{cardId}/block
```

---

## 5. Module Wallet

### 5.1 Création d'un wallet

```bash
POST /api/wallet
{
  "partyId": "{partyId}",
  "initialLevel": "Standard",
  "supportedCurrencies": ["MAD", "EUR", "USD"]
}
```

Niveaux disponibles :
- `Basic` - Limites basiques
- `Standard` - Limites moyennes
- `Premium` - Limites élevées

### 5.2 Consulter le wallet

```bash
GET /api/wallet/{walletId}
```

Réponse :
```json
{
  "id": "...",
  "partyId": "...",
  "walletLevel": "Standard",
  "status": "Active",
  "balances": [
    { "currencyCode": "MAD", "balanceMinorUnits": 500000, "balanceDecimal": 5000.00 },
    { "currencyCode": "EUR", "balanceMinorUnits": 20000, "balanceDecimal": 200.00 },
    { "currencyCode": "USD", "balanceMinorUnits": 10000, "balanceDecimal": 100.00 }
  ]
}
```

### 5.3 Plafonds

```bash
GET /api/wallet/{walletId}/limits
```

Réponse :
```json
[
  {
    "limitType": "CashIn",
    "currencyCode": "MAD",
    "dailyLimitMinorUnits": 1000000,
    "monthlyLimitMinorUnits": 10000000,
    "dailyUsedMinorUnits": 250000,
    "monthlyUsedMinorUnits": 750000
  }
]
```

Types de limites :
- `CashIn` - Dépôt
- `CashOut` - Retrait
- `P2PSend` - Envoi P2P
- `P2PReceive` - Réception P2P
- `MerchantPay` - Paiement marchand
- `BillPay` - Paiement facture

### 5.4 P2P Transferts

#### Envoyer de l'argent
```bash
POST /api/wallet/p2p/send
{
  "fromWalletId": "{walletId}",
  "toIdentifier": "+212612345679",
  "identifierType": "Phone",
  "currencyCode": "MAD",
  "amountMinorUnits": 25000,
  "description": "Remboursement déjeuner",
  "idempotencyKey": "p2p-001"
}
```

Types d'identifiant :
- `Phone` - Numéro de téléphone
- `Email` - Adresse email
- `WalletId` - ID du wallet

#### Demander de l'argent
```bash
POST /api/wallet/p2p/request
{
  "fromWalletId": "{walletId}",
  "toIdentifier": "+212612345679",
  "identifierType": "Phone",
  "currencyCode": "MAD",
  "amountMinorUnits": 50000,
  "description": "Participation cadeau"
}
```

#### Répondre à une demande
```bash
POST /api/wallet/p2p/respond
{
  "requestId": "{requestId}",
  "response": "Accept"  // ou "Reject"
}
```

### 5.5 Split Payment

```bash
POST /api/wallet/split
{
  "initiatorWalletId": "{walletId}",
  "participantIdentifiers": ["+212612345679", "+212612345680"],
  "currencyCode": "MAD",
  "totalAmountMinorUnits": 90000,
  "description": "Dîner entre amis"
}
```

Payer sa part :
```bash
POST /api/wallet/split/{splitId}/pay/{walletId}
```

### 5.6 Programmation de paiements

```bash
POST /api/wallet/scheduled
{
  "walletId": "{walletId}",
  "paymentType": "Bill",
  "currencyCode": "MAD",
  "amountMinorUnits": 50000,
  "frequency": "Monthly",
  "startDate": "2024-02-01T00:00:00Z",
  "recipientIdentifier": "FACTURE_EDM"
}
```

Fréquences :
- `Once` - Une fois
- `Daily` - Quotidien
- `Weekly` - Hebdomadaire
- `Monthly` - Mensuel

### 5.7 Programme de fidélité

#### Consulter les points
```bash
GET /api/wallet/{walletId}/loyalty
```

Réponse :
```json
{
  "walletId": "...",
  "availablePoints": 1250,
  "lifetimePoints": 5000,
  "tier": "Silver",
  "tierProgress": 0.65
}
```

Tiers :
- `Bronze` - Démarrage
- `Silver` - 1000 points
- `Gold` - 5000 points
- `Platinum` - 10000 points

#### Convertir des points
```bash
POST /api/wallet/{walletId}/loyalty/redeem
500  // points à convertir
```

Règle : 100 points = 10 MAD

### 5.8 NFC / Paiement sans contact

#### Générer un token
```bash
POST /api/wallet/{walletId}/nfc-token
```

Réponse :
```json
{
  "token": "a1b2c3d4e5f6...",
  "expiresAt": "2024-01-15T14:35:00Z",
  "status": "Active"
}
```

Validité : 5 minutes

---

## 6. Paiements

### 6.1 Virements

#### Intra-devise
```bash
POST /api/payments/transfer
{
  "fromAccountId": "{accountId}",
  "toAccountId": "{beneficiaryId}",
  "currencyCode": "MAD",
  "amountMinorUnits": 50000,
  "description": "Paiement facture",
  "idempotencyKey": "transfer-001"
}
```

#### Cross-devise (avec FX)
```bash
POST /api/payments/cross-currency-transfer
{
  "fromAccountId": "{madAccountId}",
  "toAccountId": "{eurAccountId}",
  "fromCurrencyCode": "MAD",
  "toCurrencyCode": "EUR",
  "amountMinorUnits": 100000,
  "idempotencyKey": "fx-transfer-001"
}
```

### 6.2 FX (Change de devises)

#### Obtenir un taux
```bash
GET /api/fx/rate?fromCurrencyCode=MAD&toCurrencyCode=EUR
```

Réponse :
```json
{
  "fromCurrencyCode": "MAD",
  "toCurrencyCode": "EUR",
  "rate": 0.091,
  "inverseRate": 10.989,
  "effectiveAt": "2024-01-15T10:30:00Z"
}
```

#### Créer une quote
```bash
POST /api/fx/quote
{
  "fromCurrencyCode": "MAD",
  "toCurrencyCode": "EUR",
  "amountMinorUnits": 100000
}
```

Réponse :
```json
{
  "quoteId": "...",
  "fromCurrencyCode": "MAD",
  "toCurrencyCode": "EUR",
  "sourceAmountMinorUnits": 100000,
  "targetAmountMinorUnits": 9000,
  "rate": 0.091,
  "feeMinorUnits": 91,
  "netAmountMinorUnits": 8909,
  "validUntil": "2024-01-15T10:35:00Z"
}
```

#### Exécuter la conversion
```bash
POST /api/fx/convert
{
  "quoteId": "{quoteId}",
  "sourceAccountId": "{walletId}",
  "targetAccountId": "{bankAccountId}",
  "idempotencyKey": "conv-001"
}
```

### 6.3 Paiement de factures

```bash
POST /api/payments/bill-pay
{
  "fromAccountId": "{accountId}",
  "billType": "Electricity",
  "billReference": "FACTURE_EDM_001",
  "currencyCode": "MAD",
  "amountMinorUnits": 35000,
  "idempotencyKey": "bill-001"
}
```

Types de factures :
- `Electricity` - Électricité
- `Water` - Eau
- `Internet` - Internet
- `Phone` - Téléphone

### 6.4 Recharges

```bash
POST /api/payments/top-up
{
  "fromAccountId": "{accountId}",
  "topUpType": "Mobile",
  "recipientNumber": "+212612345679",
  "currencyCode": "MAD",
  "amountMinorUnits": 2000,
  "idempotencyKey": "topup-001"
}
```

### 6.5 Ordres permanents (Standing Orders)

```bash
POST /api/payments/standing-orders
{
  "fromAccountId": "{accountId}",
  "toAccountId": "{beneficiaryId}",
  "currencyCode": "MAD",
  "amountMinorUnits": 50000,
  "frequency": "Monthly",
  "startDate": "2024-02-01T00:00:00Z",
  "endDate": "2024-12-31T00:00:00Z",
  "description": "Loyer mensuel"
}
```

Gestion :
```bash
# Lister
GET /api/payments/standing-orders?accountId={accountId}

# Annuler
DELETE /api/payments/standing-orders/{standingOrderId}
```

### 6.6 Bénéficiaires

#### Ajouter
```bash
POST /api/payments/beneficiaries
{
  "ownerPartyId": "{partyId}",
  "name": "Electricité du Maroc",
  "beneficiaryType": "BillPay",
  "identifier": "FACTURE_EDM",
  "bankName": null
}
```

Types :
- `IBAN` - Virement bancaire
- `WalletId` - Wallet
- `PhoneNumber` - Téléphone
- `BillPay` - Paiement facture

#### Lister
```bash
GET /api/payments/beneficiaries?partyId={partyId}
```

---

## 7. Compliance & Sécurité

### 7.1 eKYC (Particuliers)

#### Soumission
```bash
POST /api/compliance/kyc
{
  "partyId": "{partyId}",
  "documentType": "NationalId",
  "documentNumber": "AB123456",
  "documentExpiryDate": "2029-12-31T00:00:00Z",
  "documentFrontImageUrl": "https://storage.../front.jpg",
  "documentBackImageUrl": "https://storage.../back.jpg",
  "selfieImageUrl": "https://storage.../selfie.jpg"
}
```

Types de documents :
- `NationalId` - Carte d'identité nationale
- `Passport` - Passeport
- `ResidencePermit` - Titre de séjour

#### Workflow
```
Draft → Submitted → InReview → Approved/Rejected
```

#### Vérification statut
```bash
GET /api/compliance/kyc/status?partyId={partyId}
```

### 7.2 KYB (Entreprises)

```bash
POST /api/compliance/kyb
{
  "partyId": "{partyId}",
  "businessType": "SARL",
  "registrationNumber": "RC123456",
  "taxId": "ICE123456789",
  "registrationDate": "2020-01-15T00:00:00Z",
  "registrationDocumentUrl": "https://...",
  "articlesOfAssociationUrl": "https://...",
  "beneficialOwners": [
    {
      "firstName": "Ahmed",
      "lastName": "Benali",
      "dateOfBirth": "1980-05-15T00:00:00Z",
      "nationality": "MA",
      "ownershipPercentage": 51
    }
  ]
}
```

### 7.3 AML Screening

```bash
POST /api/compliance/aml/screen?partyId={partyId}
```

Réponse :
```json
{
  "partyId": "...",
  "riskLevel": "Low",
  "riskScore": 10,
  "hits": []
}
```

Niveaux de risque :
- `Low` - Faible (0-30)
- `Medium` - Moyen (31-70)
- `High` - Élevé (71-100)

### 7.4 Actions fortes

#### Geler un party
```bash
POST /api/compliance/strong-actions
{
  "actionType": "FreezeParty",
  "partyId": "{partyId}",
  "reason": "Suspicion de fraude",
  "initiatedBy": "admin001"
}
```

Types d'actions :
- `FreezeParty` - Geler le party
- `MarkSuspicious` - Marquer comme suspect
- `OrderAccountClosure` - Ordonner fermeture compte

#### Consulter les cas fraude
```bash
GET /api/compliance/fraud-cases?partyId={partyId}
```

---

## 8. Rapports & Relevés

### 8.1 Ledger (Historique)

#### Balances multi-devise
```bash
GET /api/ledger/accounts/{accountId}/balances
```

Réponse :
```json
{
  "accountId": "...",
  "balances": [
    { "currencyCode": "MAD", "amountMinorUnits": 1000000, "amountDecimal": 10000.00, "currencyNumericCode": 504 },
    { "currencyCode": "EUR", "amountMinorUnits": 50000, "amountDecimal": 500.00, "currencyNumericCode": 978 }
  ]
}
```

#### Historique des écritures
```bash
POST /api/ledger/accounts/{accountId}/history
{
  "currencyCode": "MAD",
  "fromDate": "2024-01-01T00:00:00Z",
  "toDate": "2024-01-31T23:59:59Z",
  "skip": 0,
  "take": 50
}
```

### 8.2 Relevés

#### Générer un relevé
```bash
POST /api/statements/generate
{
  "accountId": "{accountId}",
  "statementType": "Monthly",
  "currencyCode": "MAD",
  "fromDate": "2024-01-01T00:00:00Z",
  "toDate": "2024-01-31T23:59:59Z"
}
```

#### Exporter en PDF
```bash
POST /api/statements/export
{
  "statementId": "{statementId}",
  "format": "PDF"
}
```

Formats : `PDF`, `CSV`, `Excel`

### 8.3 Budgeting

#### Créer un budget
```bash
POST /api/budgeting/budgets
{
  "partyId": "{partyId}",
  "categoryId": "restaurants",
  "currencyCode": "MAD",
  "amountLimitMinorUnits": 200000,
  "period": "Monthly",
  "startDate": "2024-01-01T00:00:00Z",
  "isAlertEnabled": true,
  "alertThresholdPercentage": 80
}
```

Catégories :
- `restaurants`, `transport`, `shopping`, `utilities`, `healthcare`, `entertainment`, etc.

#### Analytics de dépenses
```bash
GET /api/budgeting/analytics?partyId={partyId}&currencyCode=MAD&fromDate=2024-01-01&toDate=2024-01-31
```

---

## 9. FAQ & Dépannage

### Q: Comment réinitialiser mon mot de passe ?
```bash
POST /api/auth/forgot-password
{ "email": "votre@email.com" }
```

### Q: Pourquoi ma transaction est-elle refusée ?
Vérifier :
1. Solde suffisant
2. Plafonds non atteints
3. KYC/KYB approuvé
4. Compte non gelé

### Q: Comment activer le NFC ?
```bash
POST /api/wallet/{walletId}/nfc-token
```

### Q: Quels sont les plafonds ?
```bash
GET /api/wallet/{walletId}/limits
```

### Q: Comment contacter le support ?
- Email : support@finitech.ma
- Téléphone : +212 522 123 456
- Agences : Voir `/api/branches`

---

## Annexes

### A. Codes de devise ISO 4217

| Code | Numérique | Nom |
|------|-----------|-----|
| MAD | 504 | Dirham marocain |
| EUR | 978 | Euro |
| USD | 840 | Dollar américain |
| GBP | 826 | Livre sterling |

### B. Codes erreur HTTP

| Code | Signification |
|------|---------------|
| 200 | OK |
| 400 | Requête invalide |
| 401 | Non authentifié |
| 403 | Non autorisé |
| 404 | Ressource non trouvée |
| 409 | Conflit (idempotence) |
| 422 | Entité non traitable |
| 500 | Erreur serveur |

### C. Format des montants

Tous les montants sont en **minor units** (entier) pour éviter les erreurs de précision :
- 10000 = 100.00 MAD
- 5000 = 50.00 EUR
- 100 = 1.00 USD

Conversion : `decimal = minorUnits / 10^decimalPlaces`

---

**Document version 1.0 - Finitech**
