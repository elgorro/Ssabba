# Security policy

## Supported versions

Ssabba is early software. Only the latest release on `main` receives security fixes.

## Reporting a vulnerability

Report privately through
[GitHub security advisories](https://github.com/elgorro/Ssabba/security/advisories/new).
Please do not open a public issue for a vulnerability.

Include what you can: affected version or commit, how the instance is deployed, reproduction steps,
and the impact you believe it has. You will get an acknowledgement within a few days, and an
advisory with credit once a fix is available.

## Notes for operators

- Never weaken `Oidc:RequireHttpsMetadata` outside development.
- Every `change-me` value in `deploy/.env.example` is a secret and must be replaced before exposing
  an instance to the internet.
- `deploy/.env`, ACME storage and anything under `deploy/**/data/` must stay out of version control.
- Issue the CrowdSec bouncer key after the first start; without it Traefik's bouncer is inert.
