---
title: Support
weight: 35
---

# Support

Ssabba is self-hosted, which means there is no support desk and no account manager. There are two
quite different kinds of problem, and it saves everybody time to tell them apart.

## Something is wrong with my club's instance

That is the **operator's** to fix — the person who runs the server. Most of it is in
[Self-hosting]({{< relref "../self-hosting" >}}): the containers, the database, the certificates, the
Keycloak realm, the backups. Before reporting anything upstream, check `docker compose logs -f web`
and the Grafana dashboard; a surprising share of "the app is broken" is an expired certificate or a
Keycloak issuer that the browser and the container disagree about.

Members of a club report things **to their own organisers**, in the app. See
[Safety and moderation]({{< relref "../experience/safety" >}}) — feedback, including anonymous
feedback, and abuse reports both stay on the instance and are triaged by the people who run it.

## Something is wrong with Ssabba itself

That goes to the project on GitHub, by hand. **Ssabba never transmits anything upstream.** There is
no telemetry, no crash reporting, and no in-app channel that sends a message to the project — adding
one would quietly turn every self-hosted instance into a reporting endpoint for somebody else's
server. If the project should hear about something, a person has to say so.

| What | Where |
| --- | --- |
| A bug | A GitHub issue, using the bug report template |
| A feature you want | A GitHub issue, using the feature request template — it asks for the situation you are in, not only the feature |
| Something wrong or missing in this wiki | A GitHub issue, using the docs template |
| Trouble running the stack | The self-hosting template |
| A question, or a half-formed idea | GitHub Discussions — the templates route you there |
| A security vulnerability | A **private security advisory**, never a public issue |

Blank issues are disabled on purpose: the templates ask the questions that would otherwise be the
first three replies.

## Before you file

- Say which version, and how you deploy — `compose.yaml` as published, or your own arrangement.
- Include the logs. Serilog writes structured JSON; the relevant lines are worth more than a
  description of them.
- Say what you expected. For anything touching ratings, sessions or money, the expectation is
  usually the disagreement.

## What is not supported

Modified deployments are welcome — the licence is AGPL-3.0 and exists precisely so changes stay
available — but the project cannot debug a fork's schema. Nor can it help with a Keycloak, Traefik
or Postgres configuration that departs from the published one; those projects have their own
documentation and their own communities.
