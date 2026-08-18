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

## Your community

Everything Ssabba stores belongs to a **community**: your club, your regular round, your beach. The
first person to sign in is taken to a short form at `/setup` — a name, a time zone, a currency — and
becomes the community's **owner**. That happens once; everyone who signs in afterwards simply joins
as a member, and an owner or admin can rename the community later at `/community`. Renaming is safe:
the identifier other instances know you by is assigned once and never changes.

Two settings on that page decide who may do what. **Roles** — guest, member, organiser, admin,
owner — carry the permissions; a membership that is pending or suspended carries none, whatever its
role. The **correction window** says how long the people who played may go on fixing a result of
their own before it belongs to the organisers: 60 hours unless you say otherwise, and zero if you
would rather corrections went through an organiser from the start.

One community per instance is not merely the expected setup, it is the supported one. Two groups
that want to see each other's results federate, each on its own instance — see
[Concept]({{< relref "../concept" >}}).

## Accounts and players

Keycloak knows accounts; Ssabba knows **players**. Signing in for the first time creates the player
behind your account, named after your Keycloak username, and adds you to the community.

An organiser can also enter someone by hand — useful for the regular who has never signed in, so
they can still appear on a match sheet. Those two are not joined up automatically: if that person
later gets an account, signing in gives them a *new* player rather than quietly handing their
account the rating and match history of the hand-entered one. Merging the two is a deliberate step,
not a name match.

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
