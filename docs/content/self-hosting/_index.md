---
title: Self-hosting
weight: 20
---

# Self-hosting Ssabba

## Requirements

- A machine with Docker and the Compose plugin.
- A DNS record pointing at it (needed for Let's Encrypt certificates).
- Ports 80 and 443 reachable from the internet.

## First run

```bash
git clone https://github.com/OWNER/ssabba.git
cd ssabba/deploy
cp .env.example .env
$EDITOR .env          # set APP_DOMAIN, ACME_EMAIL and every change-me secret
docker compose -f compose.yaml up -d --build
```

The app applies its database migrations on startup, so upgrades are a `docker compose pull &&
docker compose up -d`.

## Services

| Service    | Purpose                                    | URL                          |
| ---------- | ------------------------------------------ | ---------------------------- |
| `traefik`  | TLS termination, routing, CrowdSec bouncer | —                            |
| `crowdsec` | Detects and blocks abusive traffic         | —                            |
| `web`      | The Blazor app                             | `https://APP_DOMAIN/`        |
| `docs`     | This wiki                                  | `https://APP_DOMAIN/docs/`   |
| `keycloak` | Sign-in (OIDC)                             | `https://APP_DOMAIN/auth/`   |
| `grafana`  | Logs and dashboards                        | `https://APP_DOMAIN/grafana` |
| `db`       | PostgreSQL 18                              | internal only                |
| `loki` / `promtail` | Log storage and shipping          | internal only                |

## CrowdSec bouncer key

The Traefik plugin and CrowdSec must share an API key. Generate one, put it in `.env` as
`CROWDSEC_LAPI_KEY`, and restart:

```bash
docker compose exec crowdsec cscli bouncers add traefik
docker compose up -d traefik crowdsec
docker compose exec crowdsec cscli metrics
```

## Backups

Everything durable lives in the `db-data` volume. A nightly dump is enough:

```bash
docker compose exec -T db pg_dump -U ssabba ssabba | gzip > ssabba-$(date +%F).sql.gz
```
