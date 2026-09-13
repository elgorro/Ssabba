---
title: Requirements
weight: 1
---

# Requirements

What you need installed before `dotnet build` will work, and what the test suite expects beyond that.

## The essentials

- **.NET 10 SDK** (`10.0.x` — what CI pins). Every project targets `net10.0`. The solution file is
  `Ssabba.slnx`, the XML format, so an SDK old enough not to understand it will simply fail to open
  the solution.
- **Git, cloned with submodules.** The wiki theme is one, and without it the docs build produces an
  unthemed site:

  ```bash
  git clone --recurse-submodules https://github.com/OWNER/ssabba.git
  git submodule update --init --recursive   # if you already cloned without it
  ```

- **Docker with the Compose plugin**, for two unrelated reasons: it runs the development stack, and
  the integration tests start a real PostgreSQL container. If you have no container runtime you can
  still work — see [Tests](#tests).

No `dotnet workload install` step is needed. The WebAssembly client builds with the SDK as shipped;
`wasm-tools` is not required on .NET 10.

There is no Node or npm anywhere in this repository. If you find yourself looking for a
`package.json`, it does not exist.

## Tools

`dotnet ef` is a global tool and is not restored for you. Install it to match EF Core, which is at
`10.0.11` in `Directory.Packages.props`:

```bash
dotnet tool install --global dotnet-ef
```

`dotnet format` ships with the SDK, and CI runs `dotnet format Ssabba.slnx --verify-no-changes`, so
treat a clean format as part of "it builds".

**Hugo** you only need if you want to build the wiki without Docker: extended **0.164.0** (the theme
declares a minimum of 0.158.0). The development stack serves it in a container with live reload
instead, which is the easier path.

## Tests

The suite is in two tiers. Everything carrying
`[Trait(TestCategories.Category, TestCategories.Integration)]` starts a PostgreSQL container through
Testcontainers and needs Docker running:

```bash
dotnet test Ssabba.slnx                                       # everything
dotnet test Ssabba.slnx --filter "Category!=Integration"      # no container runtime required
```

The second form is the escape hatch: the domain tests are pure and will run anywhere.

## Local hosts entry

Add this to `/etc/hosts`:

```
127.0.0.1 keycloak
```

The browser and the app container have to arrive at the same OIDC issuer URL, and in development that
name is how they agree on it. The reasoning is in
[Self-hosting]({{< relref "../self-hosting" >}}); `Oidc:MetadataAddress` exists for setups where it
cannot be arranged.

## Ports the development stack publishes

`compose.override.yaml` applies automatically to `docker compose up` in `deploy/`, and publishes what
the production stack keeps internal:

| Service | Port | Override |
| --- | --- | --- |
| `db` | 5432 | `POSTGRES_PORT` |
| `keycloak` | 8080 | `KEYCLOAK_PORT` |
| `web` | 5080 | `WEB_PORT` |
| `docs` | 1313 | `DOCS_PORT` |
| `grafana` | 3000 | `GRAFANA_PORT` |

`appsettings.Development.json` expects Postgres and Keycloak at the first two.

## Two things that look like mistakes

`Directory.Build.props` sets `AllowMissingPrunePackageData`. That is there because Arch Linux's dotnet
packaging ships no ASP.NET Core prune data, not because something is misconfigured — leave it.

Package versions live in `Directory.Packages.props` under central package management. Never put a
`Version=` attribute on a `PackageReference`; the build will tell you off, and in Release it is an
error.

Versions on this page come from `Directory.Build.props`, `Directory.Packages.props` and
`.github/workflows/ci.yml`, which are the source of truth as they move.
