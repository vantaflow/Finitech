# Finitech - Manuel Développeur

## Guide de développement et architecture

---

## Table des matières

1. [Architecture](#1-architecture)
2. [Structure des projets](#2-structure-des-projets)
3. [Patterns et principes](#3-patterns-et-principes)
4. [Développement d'un module](#4-développement-dun-module)
5. [Tests](#5-tests)
6. [CI/CD](#6-cicd)
7. [Déploiement production](#7-déploiement-production)
8. [Monitoring et observabilité](#8-monitoring-et-observabilité)
9. [Sécurité](#9-sécurité)
10. [Référence API](#10-référence-api)

---

## 1. Architecture

### 1.1 Choix architectural : Modular Monolith

**Pourquoi pas les microservices ?**

```
┌─────────────────────────────────────────────────────────────┐
│                    MODULAR MONOLITH                         │
├─────────────────────────────────────────────────────────────┤
│  ✅ Transactions ACID fortes (ledger immuable)               │
│  ✅ Latence faible (pas d'appels réseau inter-services)      │
│  ✅ Simplicité opérationnelle                                │
│  ✅ Migration progressive possible vers microservices        │
│  ✅ Débogage simplifié                                       │
└─────────────────────────────────────────────────────────────┘
```

**Frontières strictes :**
- Banking ↔ Wallet : ❌ Interdiction de référence croisée
- Communication via : ✅ Contracts + Interfaces

### 1.2 Règles de dépendances

```
Domain
    ↑
Application ← Contracts
    ↑
Infrastructure
    ↑
API (Controllers)
```

**Règles :**
1. Domain ne dépend de rien (sauf SharedKernel)
2. Application dépend de Domain + Contracts
3. Infrastructure dépend de Domain
4. API dépend de Application + Contracts

### 1.3 Communication inter-modules

```csharp
// ❌ INTERDIT : Référence directe
// Dans Banking.Domain :
using Finitech.Modules.Wallet.Domain;  // ERREUR !

// ✅ AUTORISÉ : Via Contracts
// Dans Banking.Application :
using Finitech.Modules.Wallet.Contracts;

public class CreateBankAccountHandler
{
    private readonly IWalletService _walletService;  // Via interface
}
```

---

## 2. Structure des projets

### 2.1 Organisation

```
/src
  /BuildingBlocks
    /Domain        → Interfaces, Repositories, Results
    /Application   → CQRS, MediatR
    /Infrastructure→ EF Core, Messaging
    /Contracts     → DTOs partagés
    /SharedKernel  → Money, Entity, ValueObject

  /Modules
    /{ModuleName}
      /Domain         → Entities, ValueObjects, DomainEvents
      /Application    → Handlers, DTOs, Validators
      /Infrastructure → DbContext, Repositories, Migrations
      /Contracts      → Interfaces publiques, DTOs
      /Tests          → Unit tests du module

  /ApiHost
    → Composition root, DI registration
```

### 2.2 Création d'un nouveau module

```bash
# Script de création de module
MODULE_NAME=NewModule

dotnet new classlib -n Finitech.Modules.${MODULE_NAME}.Domain -o src/Modules/${MODULE_NAME}/Domain
dotnet new classlib -n Finitech.Modules.${MODULE_NAME}.Application -o src/Modules/${MODULE_NAME}/Application
dotnet new classlib -n Finitech.Modules.${MODULE_NAME}.Infrastructure -o src/Modules/${MODULE_NAME}/Infrastructure
dotnet new classlib -n Finitech.Modules.${MODULE_NAME}.Contracts -o src/Modules/${MODULE_NAME}/Contracts

# Ajouter références
# Domain → BuildingBlocks.SharedKernel
# Application → Domain + Contracts
# Infrastructure → Domain + BuildingBlocks.Infrastructure
```

---

## 3. Patterns et principes

### 3.1 Domain-Driven Design (DDD)

#### Entity
```csharp
public class Account : Entity, IAggregateRoot
{
    private readonly List<LedgerEntry> _entries = new();

    public Guid PartyId { get; private set; }
    public Money Balance { get; private set; }
    public AccountStatus Status { get; private set; }

    // Comportement métier encapsulé
    public void Credit(Money amount)
    {
        if (Status != AccountStatus.Active)
            throw new DomainException("Account not active");

        Balance = Balance.Add(amount);
        AddDomainEvent(new AccountCreditedEvent(Id, amount));
    }

    public void Debit(Money amount)
    {
        if (Balance < amount)
            throw new DomainException("Insufficient balance");

        Balance = Balance.Subtract(amount);
        AddDomainEvent(new AccountDebitedEvent(Id, amount));
    }
}
```

#### Value Object
```csharp
public class Money : ValueObject
{
    public long AmountMinorUnits { get; }
    public Currency Currency { get; }

    private Money() { }  // EF Core

    public Money(long amountMinorUnits, Currency currency)
    {
        AmountMinorUnits = amountMinorUnits;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add different currencies");

        return new Money(AmountMinorUnits + other.AmountMinorUnits, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AmountMinorUnits;
        yield return Currency;
    }
}
```

#### Domain Event
```csharp
public class AccountCreditedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public Money Amount { get; }
    public DateTime CreditedAt { get; }

    public AccountCreditedEvent(Guid accountId, Money amount)
    {
        AccountId = accountId;
        Amount = amount;
        CreditedAt = DateTime.UtcNow;
    }
}
```

### 3.2 CQRS (Command Query Responsibility Segregation)

#### Command
```csharp
public record CreateAccountCommand : ICommand<Guid>
{
    public Guid PartyId { get; init; }
    public string CurrencyCode { get; init; } = "MAD";
    public string AccountType { get; init; } = "Current";
}

public class CreateAccountHandler : ICommandHandler<CreateAccountCommand, Guid>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken ct)
    {
        var account = Account.Create(request.PartyId, request.CurrencyCode);
        await _repository.AddAsync(account);
        await _unitOfWork.SaveChangesAsync(ct);
        return account.Id;
    }
}
```

#### Query
```csharp
public record GetAccountQuery(Guid AccountId) : IQuery<AccountDto>;

public class GetAccountHandler : IQueryHandler<GetAccountQuery, AccountDto>
{
    private readonly IReadOnlyDbContext _dbContext;

    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken ct)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.AccountId)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Balance = a.Balance.AmountDecimal,
                Currency = a.Balance.Currency.Code
            })
            .FirstOrDefaultAsync(ct);
    }
}
```

### 3.3 Repository Pattern

```csharp
// Domain - Interface
public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByNumberAsync(string accountNumber);
    Task<IReadOnlyList<Account>> GetByPartyIdAsync(Guid partyId);
}

// Infrastructure - Implémentation
public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context) { }

    public async Task<Account?> GetByNumberAsync(string accountNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }
}
```

### 3.4 Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken ct = default);
}

// Usage
public async Task TransferAsync(Guid fromId, Guid toId, Money amount)
{
    await _unitOfWork.ExecuteInTransactionAsync(async ct =>
    {
        var from = await _accountRepo.GetByIdAsync(fromId);
        var to = await _accountRepo.GetByIdAsync(toId);

        from.Debit(amount);
        to.Credit(amount);

        await _accountRepo.UpdateAsync(from);
        await _accountRepo.UpdateAsync(to);

        return await _unitOfWork.SaveChangesAsync(ct);
    });
}
```

### 3.5 Result Pattern

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    public static Result<T> Success(T value) => new(value, true, Error.None);
    public static Result<T> Failure(Error error) => new(default, false, error);
}

// Usage
public async Task<Result<Guid>> Handle(CreateAccountCommand request)
{
    var existing = await _repo.GetByNumberAsync(request.AccountNumber);
    if (existing != null)
        return Result<Guid>.Failure(Error.Conflict("Account.AlreadyExists", "Account already exists"));

    var account = Account.Create(...);
    await _repo.AddAsync(account);

    return Result<Guid>.Success(account.Id);
}
```

### 3.6 Idempotence

```csharp
public class IdempotentCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly IIdempotencyService _idempotency;

    public async Task<TResult> Handle(TCommand request, CancellationToken ct)
    {
        var key = new IdempotencyKey(request.IdempotencyKey, typeof(TCommand).Name);

        var check = await _idempotency.CheckAsync(key, ct);
        if (check.IsCompleted)
            return (TResult)check.ExistingResponse;

        await _idempotency.RecordStartAsync(key, request, ct);

        try
        {
            var result = await _inner.Handle(request, ct);
            await _idempotency.RecordCompletionAsync(key, result, ct);
            return result;
        }
        catch (Exception ex)
        {
            await _idempotency.RecordFailureAsync(key, ex.Message, ct);
            throw;
        }
    }
}
```

---

## 4. Développement d'un module

### 4.1 Exemple : Créer un module de remises (Rebates)

#### Étape 1 : Domain

```csharp
// Domain/Entities/Rebate.cs
public class Rebate : Entity, IAggregateRoot
{
    public Guid MerchantId { get; private set; }
    public Money Amount { get; private set; }
    public RebateStatus Status { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    public static Rebate Create(Guid merchantId, Money amount, DateTime validFrom)
    {
        return new Rebate
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Amount = amount,
            Status = RebateStatus.Active,
            ValidFrom = validFrom
        };
    }

    public void Expire()
    {
        Status = RebateStatus.Expired;
        ValidTo = DateTime.UtcNow;
        AddDomainEvent(new RebateExpiredEvent(Id));
    }
}

// Domain/Repositories/IRebateRepository.cs
public interface IRebateRepository : IRepository<Rebate>
{
    Task<IReadOnlyList<Rebate>> GetActiveByMerchantAsync(Guid merchantId);
}
```

#### Étape 2 : Contracts

```csharp
// Contracts/DTOs/RebateDto.cs
public record RebateDto
{
    public Guid Id { get; init; }
    public Guid MerchantId { get; init; }
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; }
    public string Status { get; init; }
    public DateTime ValidFrom { get; init; }
}

// Contracts/IRebateService.cs
public interface IRebateService
{
    Task<RebateDto> CreateAsync(CreateRebateRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<RebateDto>> GetByMerchantAsync(Guid merchantId, CancellationToken ct = default);
    Task ExpireAsync(Guid rebateId, CancellationToken ct = default);
}
```

#### Étape 3 : Application

```csharp
// Application/Commands/CreateRebateCommand.cs
public record CreateRebateCommand : ICommand<Guid>
{
    public Guid MerchantId { get; init; }
    public long AmountMinorUnits { get; init; }
    public string CurrencyCode { get; init; }
}

public class CreateRebateHandler : ICommandHandler<CreateRebateCommand, Guid>
{
    private readonly IRebateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Guid> Handle(CreateRebateCommand request, CancellationToken ct)
    {
        var currency = Currency.FromCode(request.CurrencyCode);
        var amount = new Money(request.AmountMinorUnits, currency);

        var rebate = Rebate.Create(request.MerchantId, amount, DateTime.UtcNow);

        await _repository.AddAsync(rebate);
        await _unitOfWork.SaveChangesAsync(ct);

        return rebate.Id;
    }
}
```

#### Étape 4 : Infrastructure

```csharp
// Infrastructure/Persistence/RebateConfiguration.cs
public class RebateConfiguration : IEntityTypeConfiguration<Rebate>
{
    public void Configure(EntityTypeBuilder<Rebate> builder)
    {
        builder.HasKey(r => r.Id);

        builder.OwnsOne(r => r.Amount, money =>
        {
            money.Property(m => m.AmountMinorUnits).HasColumnName("AmountMinorUnits");
            money.Property(m => m.Currency).HasColumnName("CurrencyCode");
        });

        builder.HasIndex(r => r.MerchantId);
        builder.HasIndex(r => r.Status);
    }
}

// Infrastructure/Repositories/RebateRepository.cs
public class RebateRepository : Repository<Rebate>, IRebateRepository
{
    public RebateRepository(RebateDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Rebate>> GetActiveByMerchantAsync(Guid merchantId)
    {
        return await _dbSet
            .Where(r => r.MerchantId == merchantId && r.Status == RebateStatus.Active)
            .ToListAsync();
    }
}
```

#### Étape 5 : API

```csharp
[ApiController]
[Route("api/[controller]")]
public class RebatesController : ControllerBase
{
    private readonly IRebateService _service;

    [HttpPost]
    public async Task<ActionResult<RebateDto>> Create(CreateRebateRequest request)
    {
        var rebate = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = rebate.Id }, rebate);
    }

    [HttpGet("merchant/{merchantId:guid}")]
    public async Task<ActionResult<IReadOnlyList<RebateDto>>> GetByMerchant(Guid merchantId)
    {
        return Ok(await _service.GetByMerchantAsync(merchantId));
    }
}
```

---

## 5. Tests

### 5.1 Tests unitaires

```csharp
public class MoneyTests
{
    [Theory]
    [InlineData(1000, 500, 1500)]
    public void Add_SameCurrency_ReturnsSum(long a, long b, long expected)
    {
        var money1 = new Money(a, Currency.MAD);
        var money2 = new Money(b, Currency.MAD);

        var result = money1.Add(money2);

        result.AmountMinorUnits.Should().Be(expected);
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsException()
    {
        var money1 = new Money(1000, Currency.MAD);
        var money2 = new Money(1000, Currency.EUR);

        Assert.Throws<DomainException>(() => money1.Add(money2));
    }
}
```

### 5.2 Tests d'intégration

```csharp
public class LedgerIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public LedgerIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTransaction_InsufficientBalance_Returns422()
    {
        var request = new PostTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            CurrencyCode = "MAD",
            AmountMinorUnits = 1000000,
            EntryType = "Debit"
        };

        var response = await _client.PostAsJsonAsync("/api/ledger/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
```

### 5.3 Tests d'architecture

```csharp
public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace("*.Domain")
            .Should()
            .NotDependOnAny(Types.InCurrentDomain()
                .That()
                .ResideInNamespace("*.Infrastructure"))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
```

---

## 6. CI/CD

### 6.1 Pipeline GitHub Actions

```yaml
# .github/workflows/ci.yml
name: CI/CD

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Test
      run: dotnet test --no-build --verbosity normal

    - name: Publish
      run: dotnet publish src/ApiHost/Finitech.ApiHost -c Release -o ./publish

    - name: Docker Build
      run: |
        docker build -t finitech:${{ github.sha }} .
        docker tag finitech:${{ github.sha }} finitech:latest
```

### 6.2 Docker

```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Finitech.ApiHost.dll"]
```

---

## 7. Déploiement production

### 7.1 Checklist production

#### Base de données
- [ ] SQL Server en cluster (Always On)
- [ ] Backup automatique (full + incremental)
- [ ] Encryption TDE activé
- [ ] Connection pooling configuré
- [ ] Migrations automatiques (avec review)

#### Sécurité
- [ ] HTTPS uniquement
- [ ] Rate limiting configuré
- [ ] WAF (Web Application Firewall)
- [ ] Secrets dans Azure Key Vault / AWS Secrets Manager
- [ ] Rotation des clés JWT

#### Performance
- [ ] Redis Cluster pour cache
- [ ] CDN pour assets statiques
- [ ] Auto-scaling configuré
- [ ] Load balancer avec health checks

#### Monitoring
- [ ] Prometheus + Grafana
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Alertes (PagerDuty/Opsgenie)
- [ ] Log aggregation (ELK/Loki)

### 7.2 Kubernetes deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: finitech-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: finitech-api
  template:
    metadata:
      labels:
        app: finitech-api
    spec:
      containers:
      - name: api
        image: finitech:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: finitech-secrets
              key: db-connection
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

---

## 8. Monitoring et observabilité

### 8.1 Health checks

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddRedis(redisConnection)
    .AddRabbitMQ(rabbitConnection)
    .AddCheck<LedgerHealthCheck>("ledger");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false  // Liveness - just check app is running
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")  // Readiness - check dependencies
});
```

### 8.2 Métriques OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddEntityFrameworkCoreInstrumentation()
               .AddSource("Finitech.Ledger")
               .AddJaegerExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddPrometheusExporter();
    });

// Custom metrics
public class LedgerMetrics
{
    private readonly Counter<long> _transactionCounter;

    public LedgerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Finitech.Ledger");
        _transactionCounter = meter.CreateCounter<long>(
            "ledger.transactions",
            description: "Number of ledger transactions");
    }

    public void RecordTransaction(string currency, decimal amount)
    {
        _transactionCounter.Add(1,
            new KeyValuePair<string, object?>("currency", currency),
            new KeyValuePair<string, object?>("amount", amount));
    }
}
```

### 8.3 Logging structuré

```csharp
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});

// Usage
_logger.LogInformation(
    "Transaction processed: {TransactionId}, Amount: {Amount}, Currency: {Currency}",
    transactionId, amount, currency);

// Correlation ID
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();

    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId,
        ["UserId"] = context.User.Identity?.Name ?? "anonymous"
    }))
    {
        await next();
    }
});
```

---

## 9. Sécurité

### 9.1 Authentification JWT

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.finitech.ma";
        options.Audience = "finitech-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };

        // Events for logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                logger.LogError(context.Exception, "JWT authentication failed");
                return Task.CompletedTask;
            }
        };
    });
```

### 9.2 Autorisation RBAC

```csharp
[Authorize(Roles = "Admin,ComplianceOfficer")]
[HttpPost("compliance/strong-actions")]
public async Task<IActionResult> ExecuteStrongAction(...)

[Authorize(Policy = "CanTransfer")]
[HttpPost("payments/transfer")]
public async Task<IActionResult> Transfer(...)

// Policy configuration
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanTransfer", policy =>
        policy.RequireClaim("permissions", "transfer:write")
              .RequireRole("Consumer", "ProCustomer"));
});
```

### 9.3 Validation des entrées

```csharp
public record CreateAccountRequest
{
    [Required]
    [StringLength(100)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Range(0, 100000000)]
    public long InitialDepositMinorUnits { get; init; }
}

// FluentValidation
public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9 ]+$").WithMessage("Invalid characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .MustAsync(async (email, ct) =>
                !await repository.ExistsByEmailAsync(email, ct))
            .WithMessage("Email already registered");
    }
}
```

### 9.4 Protection contre les attaques

```csharp
// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Anti-forgery
builder.Services.AddAntiforgery();

// HSTS
app.UseHsts();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

---

## 10. Référence API

### 10.1 Codes erreur

| Code | Message | HTTP Status |
|------|---------|-------------|
| `ACCOUNT_INSUFFICIENT_BALANCE` | Solde insuffisant | 422 |
| `ACCOUNT_NOT_FOUND` | Compte introuvable | 404 |
| `ACCOUNT_FROZEN` | Compte gelé | 403 |
| `CURRENCY_NOT_SUPPORTED` | Devise non supportée | 400 |
| `KYC_NOT_APPROVED` | KYC non approuvé | 403 |
| `LIMIT_EXCEEDED` | Plafond dépassé | 429 |
| `IDEMPOTENCY_KEY_REUSE` | Clé idempotence déjà utilisée | 409 |

### 10.2 Pagination

```csharp
public record PaginatedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = new List<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
```

### 10.3 Versioning

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> GetV1()

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult> GetV2()
}
```

---

## Annexes

### A. Outils recommandés

| Outil | Usage |
|-------|-------|
| JetBrains Rider / VS Code | IDE |
| Postman / Insomnia | Test API |
| pgAdmin / Azure Data Studio | BDD |
| Redis Insight | Cache |
| Docker Desktop | Conteneurs |
| k9s | Kubernetes |

### B. Ressources

- [Microsoft .NET Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [DDD Reference](https://domainlanguage.com/ddd/reference/)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Modular Monolith](https://shopify.engineering/deconstructing-monolith-designing-software-maxim-developer-productivity)

### C. Contacts

- Architecture : arch@finitech.ma
- Support technique : dev@finitech.ma
- Sécurité : security@finitech.ma

---

**Document version 1.0 - Finitech Developer Guide**
