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

## Configuration

Production configuration is supplied through environment variables or a secret
provider. The repository does not contain connection strings, signing keys, or
deployment origins.

Required API settings:

- `ConnectionStrings__PostgreConnectionString`
- `AllowedHosts` (semicolon-separated host names; wildcards are rejected)
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey` (at least 32 characters)
- `Jwt__ExpirationMinutes`
- `Cors__AllowedOrigins__0` (repeat the numeric suffix for additional origins)
- `ReverseProxy__ForwardLimit`
- `Database__ApplyMigrations`

Add `ReverseProxy__KnownProxies__0` for each trusted proxy address. Set
`Database__ApplyMigrations=false` when migrations run as a separate deployment
job.

Required frontend settings:

- `PUBLIC_SITE_URL`
- `PUBLIC_API_BASE_URL`

For Aspire development, provide the `password`, `jwt-secret`, `jwt-issuer`,
`jwt-audience`, and `jwt-expiration-minutes` parameters through AppHost user
secrets or environment configuration. Aspire derives connection strings,
service origins, and allowed hosts from the declared resources.

start container without aspire (for persistent container aspire doesnt allow port mapping and persistent containers are acting up)
```bash
docker run --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -v ./postgres-data:/var/lib/postgresql postgres:18.3
```
