# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run the API
dotnet run --project src/Carnegie.KycAggregationApi.Api

# Add an EF Core migration
dotnet ef migrations add <Name> --project src/Carnegie.KycAggregationApi.Infrastructure --startup-project src/Carnegie.KycAggregationApi.Api

# Apply migrations manually (also runs automatically on startup)
dotnet ef database update --project src/Carnegie.KycAggregationApi.Infrastructure --startup-project src/Carnegie.KycAggregationApi.Api
```

Swagger UI is available at `https://localhost:{port}/swagger` in Development.

## Architecture

Clean Architecture split across four projects:

```
Domain          → core entity (AggregatedKycData)
Application     → interfaces + service logic (no infrastructure dependencies)
Infrastructure  → EF Core SQLite + HTTP client for external CustomerData API
Api             → ASP.NET Core controllers, middleware, DI wiring
```

**Dependency direction**: `Api → Application ← Infrastructure`, `Application → Domain`.  
`Infrastructure` implements the interfaces defined in `Application`.

### Data flow for `GET /kyc-data/{ssn}`

1. `KycController` calls `IKycAggregationService.GetAsync`
2. `KycAggregationService` checks `IKycRepository` (SQLite) for a cached record
3. On cache miss, it calls `ICustomerDataClient` — three separate HTTP calls to the external API:
   - `GET personal-details/{ssn}` → name
   - `GET contact-details/{ssn}` → addresses, emails, phones
   - `GET kyc-form/{ssn}/{date}` → key-value pairs including `tax_country` and `annual_income`
4. Aggregated result is persisted to SQLite and returned

`TaxCountryNotPresentException` is thrown (→ 404) when `tax_country` is absent from both cache and fresh data.  
`CustomerNotFoundException` is thrown (→ 404) when `personal-details` returns 404.  
All exception-to-status-code mapping lives in `ExceptionHandlingMiddleware`.

### External API

Base URL configured under `CustomerDataApi:BaseUrl` in `appsettings.json`. The production URL points to an Azure App Service in Sweden Central. Override in `appsettings.Development.json` or user-secrets for local development.

### Database

SQLite file `kyc.db` (connection string `DefaultConnection`). EF Core migrations are applied automatically at startup via `MigrateAsync()`. `AggregatedKycData.Ssn` is the primary key (upsert in `KycRepository.SaveAsync`).

### Tests

The `/tests/` solution folder exists but is empty — no test projects yet.
