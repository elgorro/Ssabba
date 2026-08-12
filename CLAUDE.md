# Ssabba — notes for Claude

Self-hostable beach volleyball tracker. .NET 10 Blazor Web App (Auto interactivity), PostgreSQL 18,
Keycloak for sign-in, Traefik + CrowdSec at the edge, Serilog JSON → Loki → Grafana, Hugo wiki at `/docs`.

## Commands

```bash
dotnet build Ssabba.slnx                 # note: .slnx, not .sln
dotnet test  Ssabba.slnx                 # everything; the integration tier needs Docker
dotnet test  Ssabba.slnx --filter "Category!=Integration"   # no container runtime required
dotnet format Ssabba.slnx                # CI runs --verify-no-changes

cd deploy && docker compose up -d        # dev stack (compose.override.yaml applies)
docker compose -f compose.yaml up -d     # production-shaped stack, no overrides
docker compose logs -f web

dotnet ef migrations add <Name> -p src/Ssabba.Infrastructure -s src/Ssabba.Web -o Migrations
```

## Layout

| Project                 | Rules                                                                  |
| ----------------------- | ---------------------------------------------------------------------- |
| `Ssabba.Domain`         | Entities and rating maths. No dependencies, no EF, no ASP.NET.          |
| `Ssabba.Shared`         | DTOs and route constants. Referenced by the WebAssembly client.         |
| `Ssabba.Infrastructure` | `SsabbaDbContext`, `IEntityTypeConfiguration`s, migrations.             |
| `Ssabba.Web`            | Blazor host, minimal API under `/api`, authentication, Serilog.         |
| `Ssabba.Web.Client`     | Components that must run in the browser. No `DbContext` access.         |

### Tests

| Project                       | Tier                                                              |
| ----------------------------- | ----------------------------------------------------------------- |
| `Ssabba.Domain.Tests`         | Pure unit tests. No I/O, no doubles — the domain has no deps.     |
| `Ssabba.Infrastructure.Tests` | `SsabbaDbContext` and migrations against a real Postgres container. |
| `Ssabba.Web.Tests`            | The real host via `WebApplicationFactory<Program>`.                |
| `Ssabba.TestSupport`          | `PostgresFixture` (Testcontainers) shared by the two above.        |

Anything needing Docker carries `[Trait(TestCategories.Category, TestCategories.Integration)]`, and
each test assembly declares its own `PostgresDatabase` collection (xUnit requires the definition to
live in the assembly that uses it).

No mocking library is used. Data access is exercised against real Postgres; the only substitutes are
hand-written fakes at external edges — `TestAuthHandler` for the Keycloak handshake, `TimeProvider`
for the clock. See issue #37 before reaching for a substitute framework.

## Conventions

- File-scoped namespaces, collection expressions, primary constructors where they read well.
- DTOs are `record`s in `Ssabba.Shared`; entities are classes in `Ssabba.Domain.Entities`.
- Query and command logic lives in `MatchQueries` (`src/Ssabba.Web/Endpoints/MatchEndpoints.cs`) so
  server-rendered components and the API return identical results. Add to it rather than duplicating.
- Anything not translatable to SQL (e.g. `string.Join` over team members) is projected first and
  composed in memory — see `MatchQueries.ListAsync`.
- Release builds treat warnings as errors. EF-generated migrations are exempted via
  `src/Ssabba.Infrastructure/Migrations/.editorconfig`.
- Package versions live in `Directory.Packages.props` (central package management); never put a
  `Version=` on a `PackageReference`.

## Auth

Backend-for-frontend: the server holds the OIDC tokens in its cookie and serialises only the
authentication state to the WebAssembly client (`AddAuthenticationStateSerialization` /
`AddAuthenticationStateDeserialization`). Never send tokens to the client.

The browser and the app container must agree on Keycloak's issuer URL. In production Traefik carries a
network alias for `APP_DOMAIN` so both resolve it; in development add `127.0.0.1 keycloak` to
`/etc/hosts`. `Oidc:MetadataAddress` exists for setups where discovery must use a different address.

## Gotchas found the hard way

- Arch's dotnet packaging has no ASP.NET Core prune data, hence
  `AllowMissingPrunePackageData` in `Directory.Build.props`.
- The official Hugo image declares `VOLUME /project`; the docs image therefore builds in `/src`.
- Postgres 18 wants its volume at `/var/lib/postgresql`, not `/var/lib/postgresql/data`.
- Traefik's *static* config is not templated — use `TRAEFIK_*` environment variables. The *dynamic*
  files are Go templates and can use `{{ env "..." }}`.
- The `aspnet` runtime image has neither curl nor wget; the web image installs curl for its healthcheck.

## Never

- Commit `deploy/.env`, ACME storage, or anything under `deploy/**/data/`.
- Weaken `Oidc:RequireHttpsMetadata` outside development.
