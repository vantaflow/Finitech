# Finitech — FinTech Platform Architecture

[![CI](https://github.com/khalilbenaz/Finitech/actions/workflows/ci.yml/badge.svg)](https://github.com/khalilbenaz/Finitech/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Modules](https://img.shields.io/badge/modules-18-blue)]()
[![Architecture](https://img.shields.io/badge/architecture-Modular%20Monolith-green)]()

Architecture .NET 8 complète pour une plateforme FinTech couvrant **Banking**, **Wallet** et **Paiements** avec séparation stricte des domaines, sécurité enterprise-grade et conformité réglementaire.

---

## Pourquoi Modular Monolith ?

| Raison | Détail |
|--------|--------|
| **Cohérence transactionnelle** | Le ledger immuable nécessite des transactions ACID strictes |
| **Performance** | Pas de latence réseau des appels inter-services |
| **Simplicité opérationnelle** | Un seul déploiement, un seul monitoring |
| **Migration progressive** | Décomposable en microservices si nécessaire |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                          API HOST                               │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────────┐  ┌─────────────┐│
│  │ Banking  │  │  Wallet  │  │ WalletFMCG   │  │   Ledger    ││
│  │  🟢      │  │  🟢      │  │  🟠          │  │   🟢        ││
│  └────┬─────┘  └────┬─────┘  └──────┬───────┘  └──────┬──────┘│
│       └─────────────┴───────────────┴──────────────────┘       │
│                              │                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐   │
│  │Payments  │  │    FX    │  │Statements│  │MerchantPay   │   │
│  │  🟡      │  │  🟡      │  │  🟠      │  │  🟡          │   │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └──────┬───────┘   │
│       └─────────────┴─────────────┘                │            │
│                     │                              │            │
│  ┌──────────┐  ┌────────────┐  ┌──────────┐  ┌────┴─────┐     │
│  │PartyReg. │  │ Identity   │  │Compliance│  │Disputes  │     │
│  │  🟢      │  │  🟢        │  │  🟠      │  │  🟡      │     │
│  └──────────┘  └────────────┘  └──────────┘  └──────────┘     │
│                                                                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐   │
│  │Notifs    │  │Budgeting │  │  Audit   │  │  Documents   │   │
│  │  🟡      │  │  🟡      │  │  🟠      │  │  🟠          │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              BuildingBlocks                             │   │
│  │   Domain · Application · Infrastructure · SharedKernel  │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘

🟢 Implemented   🟡 Service Layer   🟠 Contracts   ⚪ Scaffold
```

**Règles de dépendances strictes :**
- ❌ Banking ne référence JAMAIS Wallet ou WalletFMCG
- ❌ Wallet ne référence JAMAIS Banking
- ✅ Les deux référencent les modules communs (Ledger, Payments, FX, PartyRegistry)
- ✅ Communication inter-modules uniquement via Contracts (interfaces + DTOs)
- ✅ Domain ne dépend jamais d'Infrastructure (Clean Architecture)

---

## Les 18 Modules

### Core — Implémentés 🟢

| Module | Rôle | Fonctionnalités |
|--------|------|-----------------|
| **Ledger** | Source de vérité financière | Double-entry bookkeeping, multi-devise, outbox pattern, historique immutable |
| **Identity** | Authentification | JWT RSA-2048, refresh tokens avec rotation, MFA/TOTP, sessions |
| **Banking** | Comptes bancaires | Comptes épargne, prêts, découvert, calcul d'intérêts, cartes |
| **Wallet** | Portefeuille digital | P2P transfers, split payments, loyalty points, paiements programmés |
| **PartyRegistry** | Référentiel clients | Parties (Individual/Organization), rôles multi-domaines |

### Services — Application Layer 🟡

| Module | Rôle | Fonctionnalités |
|--------|------|-----------------|
| **Payments** | Virements | Intra-devise, cross-devise via FX, ordres permanents |
| **FX** | Change | Taux de change temps réel (simulé), quotes 5min, conversion MAD/EUR/USD |
| **MerchantPayments** | Paiement marchand | QR EMVCo dynamique, paiement par scan, ISO 4217 |
| **Disputes** | Litiges | Remboursements partiels/totaux, chargebacks |
| **Notifications** | Alertes | Routing SMS/Email/Push, templates |
| **Budgeting** | Gestion budget | Catégorisation, budgets mensuels, analytics de dépenses, alertes seuil |

### Contracts définis 🟠

| Module | Rôle |
|--------|------|
| **IdentityCompliance** | eKYC, KYB, AML, détection fraude, freeze/unfreeze |
| **Statements** | Relevés comptables périodiques |
| **WalletFMCG** | Distribution agents, cash-in/cash-out, commissions |
| **Audit** | Audit trail pour compliance |
| **Documents** | Stockage documents (S3 compatible) |
| **BranchNetwork** | Gestion des agences |
| **Scheduler** | Jobs planifiés (Quartz.NET) |

---

## Stack Technique

| Composant | Technologie |
|-----------|-------------|
| **Runtime** | .NET 8.0 |
| **Database** | SQL Server 2022 |
| **ORM** | Entity Framework Core + Migrations |
| **Auth** | JWT RSA-2048 signing |
| **Passwords** | Argon2id (OWASP recommended) |
| **Encryption** | AES-256-GCM pour PII |
| **MFA** | TOTP (Google/Microsoft Authenticator) |
| **Cards** | Tokenization PCI-compliant |
| **Background Jobs** | Quartz.NET |
| **API Docs** | OpenAPI / Swagger |
| **Tests** | xUnit + NetArchTest.Rules |
| **Observabilité** | OpenTelemetry + Prometheus |
| **Containers** | Docker + Docker Compose |
| **Orchestration** | Kubernetes (Kustomize) |
| **CI/CD** | GitHub Actions |

---

## Quick Start

### Prérequis
- Docker Desktop
- .NET 8 SDK

### 1. Démarrer SQL Server

```bash
docker-compose up -d sqlserver
```

### 2. Lancer l'application

```bash
dotnet run --project src/ApiHost/Finitech.ApiHost/Finitech.ApiHost.csproj
```

→ API : `http://localhost:5000`
→ Swagger : `http://localhost:5000/swagger`

### 3. Tout en Docker

```bash
docker-compose up --build
```

---

## Exemples d'utilisation

### Créer un client

```bash
curl -X POST http://localhost:5000/api/partyregistry \
  -H "Content-Type: application/json" \
  -d '{
    "partyType": "Individual",
    "firstName": "Ahmed",
    "lastName": "Benali",
    "email": "ahmed@example.com",
    "phoneNumber": "+212612345678",
    "initialRoles": ["Consumer", "RetailCustomer"]
  }'
```

### Obtenir un taux de change

```bash
curl -X POST http://localhost:5000/api/fx/rate \
  -H "Content-Type: application/json" \
  -d '{"fromCurrencyCode": "MAD", "toCurrencyCode": "EUR"}'
```

### Envoi P2P Wallet

```bash
curl -X POST http://localhost:5000/api/wallet/p2p/send \
  -H "Content-Type: application/json" \
  -d '{
    "fromWalletId": "{walletId}",
    "toIdentifier": "+212612345679",
    "identifierType": "Phone",
    "currencyCode": "MAD",
    "amountMinorUnits": 25000,
    "idempotencyKey": "p2p-001"
  }'
```

### Générer un QR EMVCo

```bash
curl -X POST http://localhost:5000/api/merchantpayments/qr/generate \
  -H "Content-Type: application/json" \
  -d '{
    "merchantId": "{merchantId}",
    "currencyCode": "MAD",
    "amountMinorUnits": 15000,
    "reference": "CMD-001"
  }'
```

→ Tous les endpoints : **[docs/API-REFERENCE.md](./docs/API-REFERENCE.md)**

---

## Sécurité

| Protection | Implémentation |
|-----------|---------------|
| **Authentication** | JWT RSA-2048, access token 15min, refresh 7j avec rotation |
| **Passwords** | Argon2id (OWASP) |
| **Data** | AES-256-GCM encryption pour PII |
| **Cards** | Tokenization PCI-compliant |
| **MFA** | TOTP compatible Google/Microsoft Authenticator |
| **Rate Limiting** | 100 req/min global, 5 req/5min auth |
| **Headers** | CSP, HSTS, X-Frame-Options, X-Content-Type-Options |

→ Détails : **[SECURITY.md](./SECURITY.md)**

---

## Multi-Devise

Le système supporte **MAD**, **EUR**, **USD** nativement :

- Stockage en **minor units** (long) + CurrencyCode (string)
- Conversion via module FX avec taux simulés (provider-ready pour production)
- Ledger : une écriture par devise, pas de conversion implicite
- QR EMVCo : devise numérique ISO 4217 (504=MAD, 978=EUR, 840=USD)

---

## Tests

```bash
# Tests unitaires (FX, Disputes, MerchantPayments, Budgeting, Money, Outbox)
dotnet test tests/Finitech.UnitTests

# Tests d'architecture (vérifie les dépendances inter-modules)
dotnet test tests/Finitech.ArchitectureTests

# Tests d'intégration (nécessite SQL Server)
docker-compose up -d sqlserver
dotnet test tests/Finitech.IntegrationTests
```

Les tests d'architecture vérifient automatiquement :
- ❌ Banking ne référence pas Wallet
- ❌ Wallet ne référence pas Banking
- ✅ Modules communiquent via Contracts uniquement
- ✅ Domain ne dépend pas d'Infrastructure

---

## Déploiement Kubernetes

```bash
# Staging
kubectl apply -k k8s/overlays/staging

# Production
kubectl apply -k k8s/overlays/production
```

Inclut : Deployment, Service, Ingress, ConfigMap, NetworkPolicy, ServiceAccount.

---

## Structure du projet

```
Finitech/
├── src/
│   ├── ApiHost/                    # ASP.NET Core host (controllers, config)
│   ├── BuildingBlocks/             # Shared kernel (Domain, Application, Infrastructure)
│   │   ├── Domain/                 # Interfaces, repositories, Result pattern
│   │   ├── Application/            # CQRS patterns
│   │   ├── Infrastructure/         # EF Core, JWT, Argon2, AES, Outbox
│   │   ├── SharedKernel/           # Money, Entity, ValueObject, AggregateRoot
│   │   └── Contracts/              # DTOs partagés
│   └── Modules/                    # 18 modules métier
│       ├── Banking/                # Comptes, prêts, cartes, intérêts
│       ├── Wallet/                 # P2P, loyalty, paiements programmés
│       ├── Ledger/                 # Double-entry bookkeeping
│       ├── Payments/               # Virements intra/cross-devise
│       ├── FX/                     # Change, quotes, conversion
│       ├── MerchantPayments/       # QR EMVCo
│       ├── PartyRegistry/          # Référentiel clients
│       ├── Identity*/              # Auth JWT + MFA
│       ├── IdentityCompliance/     # eKYC, AML
│       └── ...                     # Notifications, Budgeting, Disputes, etc.
├── tests/
│   ├── UnitTests/                  # 9 fichiers de tests
│   ├── ArchitectureTests/          # Tests de dépendances
│   └── IntegrationTests/           # Tests avec SQL Server
├── k8s/                            # Kubernetes (Kustomize)
├── docs/                           # API Reference
├── docker-compose.yml
├── CONTRIBUTING.md
├── SECURITY.md
└── LICENSE
```

---

## Roadmap

### ✅ Complété
- [x] Modular monolith avec 18 modules
- [x] Ledger double-entry immutable
- [x] JWT RSA-2048 + MFA/TOTP + refresh rotation
- [x] Argon2id + AES-256-GCM
- [x] Multi-devise MAD/EUR/USD
- [x] QR EMVCo dynamique
- [x] Background jobs (Quartz.NET)
- [x] 9 Application Services
- [x] Tests unitaires + architecture
- [x] CI/CD GitHub Actions
- [x] Docker + Kubernetes

### 🔄 Prochaines étapes
- [ ] Event sourcing pour le Ledger
- [ ] RabbitMQ pour outbox distribué
- [ ] API Gateway (Kong/Traefik)
- [ ] Multi-région
- [ ] Dashboard admin React

---

## Documentation

| Document | Contenu |
|----------|---------|
| [API Reference](./docs/API-REFERENCE.md) | Tous les endpoints (30+ routes) |
| [Security](./SECURITY.md) | Politique de sécurité et features |
| [Contributing](./CONTRIBUTING.md) | Setup, conventions, PR process |
| [Architecture](./ARCHITECTURE.md) | Décisions techniques détaillées |

---

## Licence

MIT — [Khalil Benazzouz](https://github.com/khalilbenaz)
