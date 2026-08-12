---
title: "ADR-0001: Base stack"
weight: 1
---

# ADR-0001: Base stack

**Status:** accepted — 2026-08-12

## Context

Ssabba should be easy to self-host by a single club or group of friends, on one small machine, without
a managed cloud. It should stay approachable for a .NET developer.

## Decision

- **Blazor Web App with Auto interactivity** (.NET 10). Server rendering keeps the first load small and
  the database close; components that benefit from running in the browser opt into WebAssembly. The
  `Ssabba.Web.Client` / `Ssabba.Shared` split keeps a later MAUI Hybrid app cheap.
- **PostgreSQL 18** via EF Core, one container, one volume to back up.
- **Traefik** for routing and certificates, with **CrowdSec** as the bouncer — a self-hosted stack is
  exposed to the internet and needs some defence without an appliance.
- **Keycloak** for sign-in. Rolling our own password storage is avoidable, and OIDC lets a club reuse
  an identity provider it already has.
- **Serilog compact JSON → Promtail → Loki → Grafana** for logs. Structured output is worth more than
  pretty console lines once something breaks in production.
- **Hugo** for the wiki, served by the same Traefik under `/docs`, built from the repository so docs
  are reviewed like code.
- **AGPL-3.0**, so improvements to a hosted instance stay available.

## Consequences

- Two identity systems never disagree, but Keycloak is a comparatively heavy container for a small
  club instance. It shares the PostgreSQL server to limit the cost.
- Auto render mode means every interactive component must work both server-side and in WebAssembly:
  no direct `DbContext` access from those components.
