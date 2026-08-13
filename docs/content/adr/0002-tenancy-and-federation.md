---
title: "ADR-0002: Tenancy and federation"
weight: 2
---

# ADR-0002: Tenancy and federation

**Status:** proposed — 2026-08-13

## Context

The natural unit of a beach volleyball rating is the group that plays together: everyone meets
everyone, so the numbers compare. Self-hosters are single clubs — [ADR-0001]({{< relref "0001-stack" >}})
sizes the whole stack for "a single club or group of friends, on one small machine".

But people do not stay inside one group. They travel, they enter tournaments with players from
other clubs, and two clubs a town apart might share a court calendar. If Ssabba only ever knows
about the one group on the local instance, that reality has nowhere to go — and retrofitting it
later means renumbering the tables that identify a community to the outside world.

## Decision

- **`Community` is the unit of ownership.** Almost every table carries a `CommunityId`. A player's
  rating, votes, dues and organising all hang off `CommunityMember`, not `Player`.
- **One community per instance is the normal deployment.** The schema permits several, and nothing
  will be added to forbid it, but the documented and supported path is one club, one instance.
- **No tenancy framework.** No tenant middleware, no global query filter, no `HasQueryFilter`.
  Isolation is explicit: each query filters by `CommunityId`, each write refuses to mix communities.
  A framework would buy safety at the price of a mechanism every contributor must understand, for a
  case most deployments never hit.
- **Federation is deferred, but its identifiers are fixed now.** `Community.PublicKeyId` and the
  `CommunityLink` entity — `TargetCommunityUri`, `TargetPublicKeyId`, `SharedSecretHash`, `Kind`
  (`SharedTournaments` / `SharedCourts` / `Full`), `Status` (`Proposed` / `Active` / `Suspended` /
  `Revoked`) — exist in the schema and are migrated. No code reads them.
- **Linking is per community pair and consent-based.** A link is proposed, confirmed by a handshake,
  scoped, and revocable by either side at any time. It is never a property of the instance, and
  there is no central directory: an instance federates because two communities agreed to, not
  because an operator flipped a switch.

## Consequences

- Community isolation is a convention, not a guarantee. It must be covered by tests and by review;
  a query that forgets its `CommunityId` filter leaks across communities silently (issue #44).
- `CommunityLink` rows are inert. They must not be surfaced in the UI or the API until a protocol
  exists, or they will promise behaviour that does not happen.
- Carrying an unused entity costs something: a table, a configuration and a migration that earn
  nothing today. The bet is that stable identifiers are cheaper now than a rename later.
- **Open question:** ratings live on the membership and are not comparable across communities, so a
  shared tournament between two linked communities has no obvious rating to apply. Whether the host
  community's rating governs, whether guests play unrated, or whether a separate cross-community
  rating exists, is undecided — and must be decided before federation ships (issue #43).
- **Open question:** consent is recorded per community (`ConsentRecord.CommunityId`). What a link
  may transmit about a person, and under whose consent, is unresolved (issue #42).
