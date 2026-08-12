---
title: Development
weight: 30
---

# Development

## Build and test

```bash
dotnet build Ssabba.slnx
dotnet test  Ssabba.slnx
```

## Running locally

Start the infrastructure, then the app from your machine:

```bash
cd deploy && docker compose up -d db keycloak
cd .. && dotnet watch --project src/Ssabba.Web
```

`appsettings.Development.json` points at `localhost` for both PostgreSQL and Keycloak.

## Database migrations

```bash
dotnet ef migrations add <Name> -p src/Ssabba.Infrastructure -s src/Ssabba.Web -o Migrations
dotnet ef database update      -p src/Ssabba.Infrastructure -s src/Ssabba.Web
```

The app also applies pending migrations at startup unless `Database:MigrateOnStartup` is `false`.

## Project layout

| Project                | Contains                                            |
| ---------------------- | --------------------------------------------------- |
| `Ssabba.Domain`        | Entities and rating maths, no dependencies           |
| `Ssabba.Shared`        | DTOs and route constants shared with the WASM client |
| `Ssabba.Infrastructure`| EF Core `DbContext`, configurations, migrations      |
| `Ssabba.Web`           | Blazor host, API endpoints, authentication           |
| `Ssabba.Web.Client`    | Components that can run in WebAssembly               |

## Render modes

The app uses **Auto** interactivity. Components that must run in the browser live in
`Ssabba.Web.Client` and reach the server through `/api/*`; server-rendered pages query the
`DbContext` directly through the same helpers in `MatchQueries`.

Authentication is a backend-for-frontend: tokens stay in the server's cookie, and the client only
receives the serialised authentication state.

## Writing docs

This wiki is Hugo. With the dev override running, `http://localhost:1313/docs/` live-reloads.
