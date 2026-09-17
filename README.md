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
dotnet ef migrations add Initial --project .\src\Users.Infrastracture\Users.Infrastracture.csproj --startup-project .\src\Host.WebApi\Host.WebApi.csproj --context UsersDbContext -o Migrations
```

start container without aspire (for persistent container aspire doesnt allow port mapping and persistent containers are acting up)
```bash
docker run --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -v ./postgres-data:/var/lib/postgresql postgres:18.3
```

## Development user seeding

In the `Development` environment, EF Core can create one `Admin`, one `User`, and one
`PremiumUser` while applying migrations. Seeding uses `UseSeeding` and `UseAsyncSeeding`,
so EF Core's migration lock protects concurrent runs. It remains disabled unless
`UserSeed:Enabled` is explicitly set to `true`. Configure the accounts with .NET User Secrets
or environment variables; no seed credentials are stored in the repository.

Each account uses these configuration keys, where `{index}` is `0`, `1`, or `2`:

```text
UserSeed:Users:{index}:Name
UserSeed:Users:{index}:Email
UserSeed:Users:{index}:Password
UserSeed:Users:{index}:Role
```

The roles must be `Admin`, `User`, and `PremiumUser`. Existing users are matched by
normalized email and left unchanged, making startup seeding safe to run repeatedly.
