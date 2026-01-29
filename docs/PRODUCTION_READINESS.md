# Guide de Production - Finitech API

## Résumé des Changements Production-Ready

Cette documentation décrit les améliorations apportées pour rendre la solution Finitech prête pour la production.

---

## 1. Entity Framework Core avec SQL Server

### Base de données
- **DbContext de base** : `FinitechDbContext` dans `BuildingBlocks.Infrastructure`
  - Gestion des domain events
  - Audit automatique (CreatedAt, UpdatedAt)
  - Gestion des transactions
  - Retry logic avec exponential backoff

### Module Ledger (Exemple implémenté)
- **LedgerDbContext** : DbContext spécifique au module Ledger
- **Entités** : `LedgerEntry`, `AccountBalance` (maintenant des AggregateRoots)
- **Repositories** : `LedgerEntryRepository`, `AccountBalanceRepository`
- **Service** : `LedgerService` avec implémentation réelle utilisant EF Core

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

### Authentification JWT
- Configuration complète dans `SecurityConfig.cs`
- Refresh token rotation
- Token revocation support
- Claims-based authorization

### Rate Limiting
- Rate limiting global : 100 requêtes/minute
- Rate limiting auth : 5 requêtes/5 minutes (endpoints sensibles)
- Rate limiting payments : 10 requêtes/minute

### CORS
- Configuration stricte pour production
- Origines whitelistées : `https://app.finitech.ma`, `https://admin.finitech.ma`

### Security Headers
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Content-Security-Policy` configuré

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

## 4. CI/CD Pipeline GitHub Actions

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

## 5. Kubernetes Manifests

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

## 6. Configuration Applications

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

## 7. Docker

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

## 8. Prochaines Étapes pour Production

### Base de données
- [ ] Créer les migrations initiales : `dotnet ef migrations add InitialCreate`
- [ ] Appliquer les migrations : `dotnet ef database update`
- [ ] Configurer SQL Server Always Encrypted pour les données sensibles
- [ ] Mettre en place des backups automatisés

### Sécurité
- [ ] Remplacer la clé JWT par un secret Azure Key Vault / AWS Secrets Manager
- [ ] Configurer HTTPS avec certificats valides
- [ ] Mettre en place un WAF (Cloudflare, AWS WAF)

### Monitoring
- [ ] Déployer OTel Collector dans le cluster
- [ ] Configurer Grafana pour les dashboards
- [ ] Mettre en place des alertes (PagerDuty, Slack)

### Infrastructure
- [ ] Créer le cluster EKS/GKE/AKS
- [ ] Configurer cert-manager pour les certificats TLS
- [ ] Mettre en place ExternalDNS
- [ ] Configurer Ingress Controller (nginx)

---

## Commandes Utiles

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

## Architecture de Déploiement

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

## Notes Importantes

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
