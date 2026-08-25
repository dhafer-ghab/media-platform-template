# Project Overview

A production-ready template for building modular monolith applications using .NET and Clean Architecture. Each module is fully isolated with its own clean architecture: application, domain, infrastructure, and presentation layers, enabling independent development while maintaining a single deployable application.

The frontend is built with Astro, providing a modern, high-performance web experience. The development environment is orchestrated using .NET Aspire, while production deployments are containerized with Docker Compose and fronted by Traefik as a reverse proxy.

## Architecture Overview

- Modular Monolith architecture
- Clean Architecture per module
- CQRS and Domain-Driven Design (DDD)
- Integration events with asynchronous messaging
- PostgreSQL and Entity Framework Core
- Docker Compose production deployment
- .NET Aspire local orchestration
- Traefik reverse proxy
- CI/CD ready

# docs

# migrations

- create migraitons, example for user module:
```powershell
dotnet ef migrations add Initial --project .\src\Users.Infrastracture\Users.Infrastracture.csproj --startup-project .\src\Host.WebApi\Host.WebApi.csproj -o Migrations
```

start container without aspire (for persistent container aspire doesnt allow port mapping and persistent containers are acting up)
```bash
docker run --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -v ./postgres-data:/var/lib/postgresql postgres:18.3
```

## Development Seeding

Seeding is disabled by default. When running the API directly, enable it and
provide the shared development password through user secrets:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet user-secrets set "SeedData:Enabled" "true" --project src/Host.WebApi
dotnet user-secrets set "SeedData:Users:DefaultPassword" "StrongLocalPassword123" --project src/Host.WebApi
dotnet run --project src/Host.WebApi
```

When running through Aspire, configure its secret parameter instead:

```powershell
dotnet user-secrets set "Parameters:seed-password" "StrongLocalPassword123" --project src/CleanModular.AppHost
```

The development fixtures create Admin, User, and PremiumUser accounts. Existing
users are skipped and never overwritten. To extend another module, implement and
register `IDatabaseMigrator` and `IDataSeeder` in that module's infrastructure
project. Migrations and seeders run in deterministic `Order`.
