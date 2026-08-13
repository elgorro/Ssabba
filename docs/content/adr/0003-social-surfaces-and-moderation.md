---
title: "ADR-0003: Social surfaces and moderation"
weight: 3
---

# ADR-0003: Social surfaces and moderation

**Status:** proposed — 2026-08-13

## Context

The schema models the administration of a club well: attendance, sets, ratings, dues, equipment,
consent. It does not model what a person *does* on the site beyond entering a result. There is no
way to react to a match, to show one to a friend who has no account, to block someone, to report
abuse, to tell the organiser something without signing your name to it, to read the group's own
rules, to find someone to play with on Thursday, or to be recognised for anything but a number.

Two of those gaps are safety obligations rather than features. An instance that lets members tag
each other in photos (`MediaSubject`) and bring strangers as plus-ones
(`SessionParticipant.IsGuestOfMemberId`) needs a way to say "not this person" and a way to escalate.
Today the only lever is `MembershipStatus.Suspended`, which is an admin action against a member of
one community — no help to the person on the receiving end, and no help at all between communities.

The risk in filling these gaps is drift. "Reactions, likes, sharing, badges" is the vocabulary of a
social network, and a social network is not what [ADR-0001]({{< relref "0001-stack" >}}) sized this
stack for, nor what the [Concept]({{< relref "../concept" >}}) argues Ssabba is for. The group is
the point. So the decisions below are as much about what these surfaces are *not* as what they are.

## Decision

- **A reaction is not a feed.** Reactions attach to something that already exists — a match, a
  session, a photo, a poll — and are read on that thing's own page. There is no aggregated
  timeline, no ordering of content by popularity, no follower graph, and no direct messaging.
  **"Like" is not a separate concept**: it is a reaction carrying the default emoji, so there is one
  mechanism rather than two.
- **Sharing is a scoped, revocable, read-only link.** A `ShareLink` mirrors `CommunityInvite` — a
  hashed single token, an expiry, a revocation — and grants a view of exactly one match or session
  to someone with no account. It is not a public profile, and it never carries contact details.
- **Blocking hides; it never rewrites.** A block suppresses the social surfaces between two people:
  reactions, discovery, partner search, invites, plus-ones. It must **not** delete match history,
  alter `MatchAppearance` rows, or move a rating. The ladder is a factual record of games that were
  played; a block that could edit it would make results falsifiable and would break the appearance
  trail the rating recalculation (issue #24) depends on. A block is player-to-player and
  instance-wide, because the harm it answers to does not respect a `CommunityId`.
- **A report about a person is not a dispute about a result.** `MatchDispute` stays exactly as it
  is. `AbuseReport` is a separate entity with its own queue, because conflating "that score is
  wrong" with "that person frightened me" serves neither.
- **Anonymity is real or it is not offered.** An anonymous `FeedbackEntry` or `AbuseReport` stores
  no author link and writes no `AuditEvent` naming the actor. A "mostly anonymous" channel is worse
  than none: it invites candour it cannot honour.
- **Feedback stays on the instance.** There is no in-app channel that transmits anything to the
  Ssabba project. Community feedback is triaged by the operator; product support is documentation
  pointing at GitHub — see [Support]({{< relref "../support" >}}).
- **Public discovery filters on a self-declared band, never on the rating.**
  [ADR-0002]({{< relref "0002-tenancy-and-federation" >}}) establishes that `CommunityMember.Rating`
  means something only inside its community. Discovery across communities therefore uses
  `SkillBand`, derived from the advisory `PlayerProfile.SelfRatedLevel`. Inside a single community
  the local rating may inform the band; outside it, it may not. This keeps the filter honest and
  keeps discovery from being blocked on issue #43.
- **Discovery is opt-in and within the instance.** A player is not discoverable until they say so
  (`PlayerProfile.DiscoveryOptIn`). Cross-instance search belongs to the federation protocol
  (issue #42) and is not designed here.
- **Badges are community-scoped and awarded, not computed.** A `Badge` belongs to a community and
  means what that group says it means. The first pass awards them by hand; automatic criteria are
  deliberately deferred. There is no points economy and no cross-community badge.
- **Two kinds of rules, kept apart.** The sport's rules are external and are only summarised and
  linked, never reproduced — see [Official rules]({{< relref "../rules/official" >}}).
  `RuleSet` and `Format` express how a group *deviates* from them. The group's own rules — conduct,
  etiquette, money, guests — are community-authored `CommunityRuleDocument` content, versioned, and
  optionally requiring acceptance.
- **Visibility gets its own enum.** `Audience` (`Private` / `Members` / `Linked` / `Public`) applies
  to `Team`, `Match` and `Session`. `ContactVisibility` is not reused, despite the precedent of
  `MediaAsset.Visibility`: naming a match's audience after contacts reads badly, and `Linked`
  — visible to federated communities, inert until issue #42 — has no counterpart there. The two
  existing `ContactVisibility` uses stay as they are.
- **Targets are referenced polymorphically.** `Reaction`, `ShareLink` and `AbuseReport` carry a
  `TargetKind` plus a bare `Guid`, with no foreign key. The alternatives were weighed:
  - **A table per target** (`MatchReaction`, `SessionReaction`, …) keeps referential integrity and
    cascade deletes, but multiplies every entity, configuration and query by the number of things a
    person can react to, and grows again each time a new one is added.
  - **One table with a nullable FK per target** keeps integrity and cascades, at the price of a
    widening row of mostly-null columns and a check constraint asserting that exactly one is set.
  - **A shared `Content` supertype** that matches, sessions and media all point at is the textbook
    answer, and would mean reshaping five existing tables to buy a property none of them needs yet.
  - **A kind plus a bare `Guid`** — chosen. One table, one configuration, one query shape, and new
    target kinds cost an enum value.

## Consequences

- **The database cannot clean up after a deleted target.** Deleting or soft-deleting a match must
  explicitly remove its reactions and revoke its share links; nothing in the schema will do it. This
  is the price of the polymorphic reference and must be covered by tests, in the same way community
  isolation is (issue #44).
- Reactions on a soft-deleted `Match`, `Session` or `MediaAsset` must be filtered out by hand, since
  there is no global query filter and no join to hang the exclusion on.
- `Audience` is a fourth visibility vocabulary in the schema, alongside `ContactVisibility`,
  `CommunityVisibility` and `VenueAccess`. That is one more thing to know; the alternative was one
  enum meaning different things in different places.
- `Audience.Linked` is inert, exactly like `CommunityLink`. It must not be offered in the UI until
  a protocol exists, or it promises behaviour that does not happen.
- Anonymous rows are unreachable by `DataRequest` erasure — they carry no `PlayerId` to match. That
  is the intended behaviour and must be stated in the privacy notice, not treated as a gap.
- Blocking is instance-wide while almost everything else is community-scoped. Every query that
  surfaces one player to another must consult it, and nothing in the schema forces that.
- **Open question:** whether a block should be visible to the blocked person, and whether an
  organiser should see that a block exists when the two are put on the same court. Silent blocking
  is kinder in the moment and can produce a scheduling puzzle nobody can explain.
- **Open question:** who moderates on a single-community instance, where the owner may be the
  subject of the report. There is no appeal above the instance operator, and self-hosting means
  there cannot be one.
