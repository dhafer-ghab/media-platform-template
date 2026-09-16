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

## Posts discovery API

The Posts module exposes the published read model used by Home, Explore, Search,
and Tags. It does not reference the Users, Storage, or Catalog modules.

- `GET /api/posts` supports `q`, `category`, `tag`, `sort`, `page`, and `pageSize`.
- `GET /api/posts/facets` returns dynamic category and tag counts.
- `GET /api/posts/{id}` returns one published post.
- `POST /api/posts/{id}/views` records a view using an atomic database update.

Valid sort values are `newest`, `popular`, `oldest`, and `title`. Public queries
never return draft or archived posts. Media upload and post-authoring commands
remain separate concerns and can supply `MediaAssetId` and `MediaUrl` without a
direct module reference.

- create migraitons, example for user module:
```powershell
dotnet ef migrations add Initial --project .\src\Users.Infrastracture\Users.Infrastracture.csproj --startup-project .\src\Host.WebApi\Host.WebApi.csproj --context UsersDbContext -o Migrations
```

start container without aspire (for persistent container aspire doesnt allow port mapping and persistent containers are acting up)
```bash
docker run --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -v ./postgres-data:/var/lib/postgresql postgres:18.3
```
