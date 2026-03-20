# Contributing to Finitech

Merci de votre intérêt pour Finitech ! Voici comment contribuer.

## Prérequis

- .NET 8.0 SDK
- Docker Desktop
- PostgreSQL 2022 (via Docker)

## Setup local

```bash
git clone https://github.com/khalilbenaz/Finitech.git
cd Finitech
docker-compose up -d postgres
dotnet restore
dotnet build
dotnet test
```

## Conventions

### Architecture
- Chaque module suit : Domain → Application → Infrastructure → Contracts
- Le Domain ne dépend de RIEN d'externe
- La communication inter-modules passe par les Contracts (interfaces + DTOs)
- Banking et Wallet ne se référencent JAMAIS mutuellement

### Code
- Nommage en anglais
- Un fichier par classe
- DTOs dans le dossier Contracts/DTOs/
- Services dans Application/Services/
- Handlers CQRS dans Application/Commands/ et Application/Queries/

### Commits
Format : `type(scope): description`
- `feat(wallet)`: nouvelle fonctionnalité
- `fix(ledger)`: correction de bug
- `refactor(banking)`: refactoring
- `test(payments)`: ajout de tests
- `docs`: documentation

### Pull Requests
1. Fork le repo
2. Crée une branche (`feat/ma-feature`)
3. Commite avec le format ci-dessus
4. Ouvre une PR vers `main`
5. Les tests doivent passer

## Structure d'un module

```
ModuleName/
├── Domain/           # Entities, ValueObjects, Events
├── Application/      # Services, Commands, Queries, Handlers
├── Infrastructure/   # EF Core, Repositories, External Services
└── Contracts/        # Interfaces, DTOs (seule partie visible par les autres modules)
```

## Tests

```bash
dotnet test tests/Finitech.UnitTests          # Tests unitaires
dotnet test tests/Finitech.ArchitectureTests   # Tests d'architecture
dotnet test tests/Finitech.IntegrationTests    # Tests d'intégration
```
