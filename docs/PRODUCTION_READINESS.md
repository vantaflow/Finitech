# Guide de Production - Finitech API

## Résumé des Changements Production-Ready

Cette documentation décrit les améliorations apportées pour rendre la solution Finitech prête pour la production.

---

## 1. Entity Framework Core avec SQL Server

### Modules avec Persistence Implémentée

#### Identity Module
- **IdentityDbContext** : Users, RefreshTokens, UserSessions, Roles, Permissions
- **Sécurité** : Password hashing Argon2id, Encryption AES-256-GCM
- **Schéma** : `identity`

#### Banking Module
- **BankingDbContext** : BankAccounts, Cards, Loans
- **PCI Compliance** : Card tokenization
- **Schéma** : `banking`

#### Wallet Module
- **WalletDbContext** : WalletAccounts, WalletBalances, WalletTransactions, ScheduledPayments
- **P2P** : Transfers et scheduled payments
- **Schéma** : `wallet`

#### Ledger Module
- **LedgerDbContext** : LedgerEntry, AccountBalance, OutboxMessages
- **Double-entry bookkeeping** : Immutable ledger
- **Outbox Pattern** : Reliable messaging
- **Schéma** : `ledger`

### Configuration Connection Strings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Finitech;User Id=sa;Password=...;TrustServerCertificate=True",
    "IdentityConnection": "...",
    "BankingConnection": "...",
    "WalletConnection": "..."
  }
}
```

### Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Finitech;User Id=sa;Password=...;TrustServerCertificate=True"
  }
}
```

---

## 2. Sécurité Production

### Authentification JWT avec RSA-2048
- **Signing** : RSA-2048 key pairs (stockés sur disque/dev, HSM en prod)
- **Access tokens** : 15 minutes
- **Refresh tokens** : 7 jours avec rotation et revocation
- **Claims** : Roles, Permissions, UserId
- **Configuration** : `JwtService` dans `BuildingBlocks.Infrastructure`

### Password Hashing (Argon2id)
- **Algorithm** : Argon2id (OWASP recommended)
- **Parameters** : m=65536 (64MB), t=3, p=4
- **Salt** : Unique par utilisateur, 128 bits
- **Verification** : Constant-time comparison
- **Migration support** : Compatible avec PBKDF2 pour migration

### Data Encryption (AES-256-GCM)
- **Algorithm** : AES-256-GCM (AEAD)
- **Key rotation** : Supporté avec key versioning
- **PII encryption** : Email, PhoneNumber, NationalId
- **Blind indexing** : Pour recherche sur données chiffrées

### Rate Limiting
- **Global** : 100 requêtes/minute
- **Auth endpoints** : 5 requêtes/5 minutes (login, register)
- **Payments** : 10 requêtes/minute
- **IP Whitelist** : Admin endpoints restreints

### CORS et Security Headers
- CORS strict pour origines whitelistées
- Security headers : CSP, HSTS, X-Frame-Options, X-Content-Type-Options
- HTTPS enforcement en production

---

## 3. Monitoring et Observabilité

### OpenTelemetry
- **Tracing** : ASP.NET Core, HttpClient, SQL Client
- **Metrics** : ASP.NET Core, Runtime, Process
- **Export** : OTLP vers collector, Prometheus endpoint

### Serilog
- Logging structuré avec contexte enrichi
- Sinks : Console, File (JSON), Seq (optionnel)
- Enrichissement : MachineName, Environment, Application

### Health Checks
- `/health/live` : Liveness probe (Kubernetes)
- `/health/ready` : Readiness probe (vérification DB)
- SQL Server health check
- Memory usage monitoring

### Métriques Prometheus
- Endpoint : `/metrics`
- Métriques applicatives et runtime

---

## 4. Background Jobs (Quartz.NET)

### Jobs Planifiés

#### InterestAccrualJob
- **Schedule** : Tous les jours à 2:00 AM
- **Module** : Ledger
- **Action** : Calcul et application des intérêts sur comptes épargne

#### ScheduledPaymentJob
- **Schedule** : Toutes les 15 minutes
- **Module** : Wallet
- **Action** : Exécution des paiements programmés (standing orders)

#### TokenCleanupJob
- **Schedule** : Tous les jours à 3:00 AM
- **Module** : Identity
- **Action** : Nettoyage des refresh tokens et sessions expirés

### Configuration
```csharp
services.AddQuartz(q => {
    q.AddJob<InterestAccrualJob>(...)
     .AddTrigger(...)
});
services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
```

---

## 5. Intégrations Externes (Mocks)

### Interfaces prêtes pour production

#### SMS Service (ISmsService)
- **Mock** : Console output
- **Production** : Twilio

#### Email Service (IEmailService)
- **Mock** : Console output
- **Production** : SendGrid

#### KYC Provider (IKycProvider)
- **Mock** : Réponses simulées
- **Production** : Jumio, Onfido, SumSub

#### Payment Gateway (IPaymentGateway)
- **Mock** : Réponses simulées
- **Production** : Stripe, Adyen

#### FX Rate Provider (IFxRateProvider)
- **Mock** : Taux fixes
- **Production** : XE, Fixer.io, ECB
- **Cache** : 5 minutes (MemoryCache)

#### Document Storage (IDocumentStorage)
- **Mock** : Système de fichiers local
- **Production** : AWS S3, Azure Blob
- **Features** : Presigned URLs (15 min expiry)

---

## 6. MFA/2FA et PCI Compliance

### MFA Service (IMfaService)
- **TOTP** : Compatible Google Authenticator, Microsoft Authenticator
- **QR Code** : Génération pour setup
- **Validation** : Time-based avec fenêtre de tolérance

### Recovery Codes
- **Génération** : 10 codes à usage unique
- **Stockage** : Hashés (même sécurité que passwords)

### Card Tokenization (ICardTokenizationService)
- **PCI DSS compliant** : PAN jamais stocké en clair
- **Format-preserving tokens** : Même format que la carte originale
- **Luhn validation** : Vérification des numéros de carte

---

## 7. CI/CD Pipeline GitHub Actions

### Workflows (`.github/workflows/ci-cd.yml`)

#### Jobs
1. **Test** : Build et exécution des tests
2. **Architecture Tests** : Vérification des règles d'architecture
3. **Security Scan** : Analyse Trivy des vulnérabilités
4. **Build & Push** : Construction et push de l'image Docker
5. **Deploy Staging** : Déploiement automatique sur staging
6. **Deploy Production** : Déploiement manuel sur production (tag v*)
7. **Database Migration** : Migrations EF Core en production

### Environnements
- **Staging** : Déclenchement sur push vers `develop`
- **Production** : Déclenchement sur tag `v*`

---

## 8. Kubernetes Manifests

### Structure Kustomize
```
k8s/
├── base/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── ingress.yaml
│   ├── serviceaccount.yaml
│   ├── configmap.yaml
│   ├── networkpolicy.yaml
│   └── kustomization.yaml
└── overlays/
    ├── staging/
    │   ├── kustomization.yaml
    │   └── deployment-patch.yaml
    └── production/
        ├── kustomization.yaml
        ├── deployment-patch.yaml
        └── ingress-patch.yaml
```

### Ressources Déployées

#### Deployment
- Replicas : 3 (min) à 20 (max avec HPA)
- Rolling update : maxSurge=1, maxUnavailable=0
- SecurityContext : runAsNonRoot, readOnlyRootFilesystem
- Resource limits et requests configurés

#### HorizontalPodAutoscaler
- Scale sur CPU (>70%) et Memory (>80%)
- Stabilization windows configurées

#### NetworkPolicy
- Ingress : uniquement depuis ingress-nginx et monitoring
- Egress : uniquement vers SQL Server, Redis, OTel Collector, DNS

#### PodDisruptionBudget
- minAvailable: 2 (garantit la disponibilité lors des mises à jour)

---

## 9. Configuration Applications

### appsettings.json
```json
{
  "ConnectionStrings": { ... },
  "Jwt": {
    "Key": "...",
    "Issuer": "Finitech",
    "Audience": "Finitech.Users",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["https://app.finitech.ma"]
  },
  "Tracing": {
    "SampleRate": 0.1  // 10% en production
  }
}
```

### Variables d'environnement importantes
- `ASPNETCORE_ENVIRONMENT`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `OTEL_EXPORTER_OTLP_ENDPOINT`
- `OTEL_SERVICE_NAME`

---

## 10. Docker

### Dockerfile
- Multi-stage build (build, publish, final)
- Image runtime : `mcr.microsoft.com/dotnet/aspnet:8.0`
- Non-root user (UID 1000)
- Read-only root filesystem

### docker-compose.yml
- SQL Server 2022
- Redis 7
- API avec health checks

---

## 11. Prochaines Étapes pour Production

### Base de données
- [x] EF Core DbContext par module (Identity, Banking, Wallet, Ledger)
- [ ] Créer les migrations initiales : `dotnet ef migrations add InitialCreate`
- [ ] Appliquer les migrations : `dotnet ef database update`
- [ ] Configurer SQL Server Always Encrypted pour les données sensibles
- [ ] Mettre en place des backups automatisés

### Sécurité
- [x] JWT avec RSA signing
- [x] Argon2id password hashing
- [x] AES-256-GCM data encryption
- [ ] Remplacer la clé JWT par un secret Azure Key Vault / AWS Secrets Manager
- [ ] Configurer HTTPS avec certificats valides
- [ ] Mettre en place un WAF (Cloudflare, AWS WAF)

### Background Jobs
- [x] Quartz.NET scheduler configuré
- [ ] Persistance Quartz (SQL Server) pour cluster support

### Monitoring
- [x] OpenTelemetry tracing et metrics
- [ ] Déployer OTel Collector dans le cluster
- [ ] Configurer Grafana pour les dashboards
- [ ] Mettre en place des alertes (PagerDuty, Slack)

### Infrastructure
- [ ] Créer le cluster EKS/GKE/AKS
- [ ] Configurer cert-manager pour les certificats TLS
- [ ] Mettre en place ExternalDNS
- [ ] Configurer Ingress Controller (nginx)

---

## 12. Commandes Utiles

### Local Development
```bash
# Démarrer l'infrastructure
docker-compose up -d

# Appliquer les migrations
dotnet ef database update --project src/Modules/Ledger/Infrastructure --startup-project src/ApiHost/Finitech.ApiHost

# Lancer l'API
dotnet run --project src/ApiHost/Finitech.ApiHost
```

### Kubernetes
```bash
# Déployer en staging
kubectl apply -k k8s/overlays/staging

# Déployer en production
kubectl apply -k k8s/overlays/production

# Voir les logs
kubectl logs -l app=finitech-api -n production --tail=100 -f

# Port-forward pour debug
kubectl port-forward svc/finitech-api 8080:80 -n production
```

### Database Migrations
```bash
# Créer une migration
dotnet ef migrations add MigrationName \
  --project src/Modules/Ledger/Infrastructure \
  --startup-project src/ApiHost/Finitech.ApiHost \
  --output-dir Data/Migrations

# Appliquer les migrations
dotnet ef database update \
  --project src/Modules/Ledger/Infrastructure \
  --startup-project src/ApiHost/Finitech.ApiHost
```

---

## 13. Architecture de Déploiement

```
┌─────────────────────────────────────────────────────────────┐
│                        Client                                │
│              (Mobile App / Web / API Consumers)             │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTPS
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                     Cloudflare / CDN                         │
│              (DDoS Protection, Caching, WAF)                │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                  Ingress Controller                          │
│              (Nginx, cert-manager TLS)                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                 Kubernetes Cluster                           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              finitech-api Pods (3-20)                │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌───────────┐  │  │
│  │  │   Container  │  │   Container  │  │   Sidecar │  │  │
│  │  │   (.NET API) │  │  (OTel Agent)│  │  (Envoy)  │  │  │
│  │  └──────────────┘  └──────────────┘  └───────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  SQL Server  │  │    Redis     │  │  OTel Collector  │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                   Observability Stack                        │
│              (Prometheus, Grafana, Jaeger)                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 14. Notes Importantes

1. **Secrets** : Ne jamais committer les secrets dans Git. Utiliser :
   - Kubernetes Secrets
   - Azure Key Vault
   - AWS Secrets Manager
   - Sealed Secrets / External Secrets Operator

2. **Migrations** : Toujours tester les migrations sur un environnement de staging avant production

3. **Monitoring** : Configurer des alertes pour :
   - CPU/Memory > 80%
   - Error rate > 1%
   - Latency p95 > 500ms
   - Database connection failures

4. **Backup** : Planifier des backups automatiques :
   - Database : quotidien avec rétention 30 jours
   - Application configuration : avant chaque déploiement
