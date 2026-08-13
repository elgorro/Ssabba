---
title: Discovery
weight: 3
---

# Discovery

Four questions, and they are always asked together: **who** is playing, **when**, **where**, and
**roughly how well**. Discovery is the one surface in Ssabba that looks outward from a community.

## Opt in first

Nobody is discoverable by default. `PlayerProfile.DiscoveryOptIn` is off until a player turns it on,
and a session appears only if its `Audience` is `Public`. A club that keeps to itself never appears
anywhere, and never has to configure that.

Discovery is also **within one instance**. Searching across federated instances depends on a
protocol that does not exist yet (issue #42), and designing the search first would be designing
against nothing.

## Looking for a game

A `PlayingInterest` is a standing note that says *I would like to play*: which formats, a time
window, a place — a venue, or a point and a radius — and the band of players it is aimed at. It
expires, because an interest posted in April should not still be matching people in September.

The search runs both ways from the same rows. A person looking for a partner and an organiser
looking to fill a session on Thursday are asking the same question of the same data.

- **By time** — an interest carries a window; sessions carry `StartsAt`/`EndsAt`. Overlap is the
  match.
- **By place** — `Venue` already carries `Latitude` and `Longitude`. A search is a radius around a
  point, defaulting to the player's `SearchRadiusKm` around their `HomeVenue`.
- **By level** — see below.
- **By format** — `Format` distinguishes 2v2 from 6v6, and turning up to the wrong one wastes an
  evening for everyone.

## Level, and why it is not the rating

`CommunityMember.Rating` is an Elo number, and
[ADR-0002]({{< relref "../adr/0002-tenancy-and-federation" >}}) is explicit that it means something
**only inside its own community**. Everyone in a group plays everyone else, so the numbers there
compare; 1240 on one beach and 1240 on another are not the same claim, and there is no exchange rate
between them. Ranking strangers by it would be inventing a comparison the model says does not exist.

So public discovery filters on a **`SkillBand`** — beginner, improver, intermediate, advanced,
competitive — derived from `PlayerProfile.SelfRatedLevel`, the advisory 1–10 a player sets for
themselves. It is coarse and it is self-reported, and both of those are the point: it is a way of
not wasting an evening, not a ranking.

Inside a single community, where the numbers do compare, the local rating may inform the band
instead. Outside it, it may not. This keeps the filter honest, and it keeps discovery from being
blocked on the unresolved cross-community rating question (issue #43).

A session may state the band it is aimed at, which is how "beginners very welcome" and "please, only
if you can pass" both become searchable rather than being buried in a free-text note.

## Blocking applies

Every discovery query has to consult blocks: a blocked player must not appear in the other's
results, in either direction. Nothing in the schema enforces this — see
[Safety and moderation]({{< relref "safety" >}}) — so it belongs in the queries and in the tests.
