---
title: Requirements
weight: 1
---

# Requirements

Everything Ssabba needs runs in containers. Nothing is installed on the host but Docker, and the
versions below are the ones the stack is built and tested against.

## Host

- **Linux, x86-64.** The published images are built for `linux/amd64` only. On another architecture
  you would be building `ssabba-web` and `ssabba-docs` yourself.
- **Docker Engine** with the **Compose v2 plugin** — the `docker compose` subcommand, not the
  standalone `docker-compose` v1 script. Docker Engine 25.0 and Compose v2.24 are a safe floor;
  nothing in the repository enforces a minimum, so treat that as a recommendation rather than a
  requirement.
- **Root or a user in the `docker` group**, since two services mount the Docker socket — see
  [What the stack asks of the host](#what-the-stack-asks-of-the-host).

## Size

There are no resource limits in `compose.yaml`, so these are guidance rather than measured figures:

| | Comfortable | Tight |
| --- | --- | --- |
| CPU | 2 vCPU | 1 vCPU |
| Memory | 4 GB | 2 GB |
| Disk | 20 GB | 10 GB |

Eight containers run at once. Keycloak is a JVM and is the single largest consumer; Postgres and the
Loki/Grafana pair come next. Traefik, CrowdSec, Promtail, the app and the wiki are small. On 2 GB you
will be relying on swap the first time Keycloak imports its realm.

Disk grows with two things: the database, which is small unless your community is enormous, and the
log store, which is not — Loki keeps whatever Promtail ships it.

## Network

- A **DNS record** for `APP_DOMAIN` pointing at the host. Let's Encrypt resolves it, so it has to be
  a real name, not a hosts-file entry.
- **Ports 80 and 443** reachable from the internet. Traefik publishes both; `HTTP_PORT` and
  `HTTPS_PORT` in `.env` move them if something else already holds them, though ACME's TLS challenge
  needs 443 to be the port the world reaches.
- **Outbound internet**, for three separate reasons: pulling images, the ACME challenge that issues
  your certificate, and the CrowdSec bouncer plugin (`v1.7.1`), which Traefik downloads when it
  starts. An air-gapped host will not bring the edge up at all.

TLS is terminated by Traefik at TLS 1.2 or better, with HSTS set to one year.

## What runs

| Service | Image |
| --- | --- |
| `traefik` | `traefik:v3.7` |
| `crowdsec` | `crowdsecurity/crowdsec:latest` |
| `db` | `postgres:18-alpine` |
| `keycloak` | `quay.io/keycloak/keycloak:26.7` |
| `web` | `mcr.microsoft.com/dotnet/aspnet:10.0` (built from `src/Ssabba.Web/Dockerfile`) |
| `docs` | `nginxinc/nginx-unprivileged:alpine` (built from `docs/Dockerfile`) |
| `loki` | `grafana/loki:3.7.6` |
| `promtail` | `grafana/promtail:3.6.11` |
| `grafana` | `grafana/grafana:13.1.3` |

CrowdSec is the one image tracking a moving tag; everything else is pinned. What each service is for,
and where it appears, is in [Services]({{< relref "_index.md#services" >}}).

## What the stack asks of the host

Two services see the Docker daemon, and it is worth deciding that deliberately rather than
discovering it later:

- **Traefik** mounts `/var/run/docker.sock` read-only, to discover which containers to route to.
- **Promtail** mounts the same socket read-only, plus `/var/lib/docker/containers` read-only, to read
  every container's log stream.

Read-only access to the socket is still enough to enumerate everything the daemon runs. Both are
standard for this shape of stack, but neither is nothing.

## Durable state

Eight named volumes, of which two are the ones you would actually miss:

- **`db-data`** — the database. Everything Ssabba stores, plus Keycloak's own.
- **`web-keys`** — the data-protection keys. Lose them and every session and antiforgery token is
  invalidated; everyone signs in again.

The rest — `traefik-acme`, `traefik-logs`, `loki-data`, `grafana-data`, `crowdsec-db`,
`crowdsec-config` — regenerate, though losing `traefik-acme` means re-issuing certificates and
re-entering Let's Encrypt's rate limits. See [Backups]({{< relref "_index.md#backups" >}}).

## Configuration

`deploy/.env.example` is the full list of what you can set, and the short answer is `APP_DOMAIN`,
`ACME_EMAIL` and every `change-me` secret. The image tags above come from `deploy/compose.yaml`,
which is the source of truth as they move.
