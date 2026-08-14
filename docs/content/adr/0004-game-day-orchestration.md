---
title: "ADR-0004: Game day orchestration"
weight: 4
---

# ADR-0004: Game day orchestration

**Status:** proposed — 2026-08-14

## Context

Ssabba can describe a session before it happens and record matches after it has. It cannot help
while it is happening, which is when the work actually is. On a full evening one person allocates
people to nets continuously for two hours: they must know who is here, who has just left, who has
not played in half an hour, and who is a reasonable set of four to put on court three. This is done
on paper, in heads, and badly at scale.

The schema is closer to this than it looks. `SessionParticipant` already separates what somebody
promised (`Response`) from what happened (`Attendance`), and its own doc comment says keeping both
is the point. `Court` and `CourtReservation` describe the nets. `TournamentType` already enumerates
`KingOfTheCourt` and the rest. What is missing is the running state of an evening and anything that
proposes a line-up.

Three risks shape the decisions. The first is **automation overreach**: a system that allocates
courts by itself is wrong in front of forty people who can all see the sand, and it will be
abandoned the first time it is stubborn. The second is **modelling the ideal club rather than a real
one** — one where everybody has the app, everybody answered the poll, and nobody brings a friend.
The third is **drift into a different product**: "AI team suggestions" is a phrase that can end with
a chat assistant bolted to a volleyball tracker.

## Decision

- **Presence becomes a state that moves, on the field that already exists.** `AttendanceState` gains
  `EnRoute`, `CheckedIn`, `Paused` and `CheckedOut` alongside `Unknown`, `Present`, `NoShow` and
  `Excused`. A **second parallel enum was rejected**: the axis Ssabba needs is exactly the one
  `Attendance` already is, and splitting it would leave two fields answering "was she there?" with
  a rule about which wins. `Present` stays, meaning "was here, nothing finer recorded", so
  retrospective data entry never has to invent a timeline.
- **Only `CheckedIn` is eligible to play.** `Paused` holds a seat and drops out of proposals;
  `CheckedOut` releases the seat. Presence beats commitment on the day: an absent registered player
  does not block a checked-in waiting one.
- **Every transition is performable by an organiser for somebody else, and records who did it.**
  This is the normal path, not a fallback. A presence change therefore carries an actor and a
  timestamp, because `NoShow` touches `ReliabilityScore` and a claim about another person must be
  answerable. The audit lives on the transition, not the participant row: the row knows the current
  state, and disputes are always about how it got there.
- **Presence never edits history.** Checking out does not void `MatchAppearance` rows or move a
  rating, and does not stop a running set. The ladder is a factual record of games played, exactly
  as [ADR-0003]({{< relref "0003-social-surfaces-and-moderation" >}}) established for blocking.
- **A session guest is a named person on one session, distinct from `CommunityMember.Role = Guest`.**
  `SessionParticipant.IsGuestOfMemberId` records that somebody brought a plus-one but not who it is,
  so two friends of the same host cannot be told apart or put on a court by name. A guest holds no
  rating, and **a match containing a guest is recorded but rated for nobody** — a result against an
  unrated stranger is not evidence about the rated players either. Promotion to membership does not
  retro-rate earlier matches; doing so would require recomputing every opponent's rating since, on
  evidence that was never rated (issue #24).
- **The board proposes; the organiser disposes.** Court assignment is a suggestion that can be
  pinned, swapped, reordered or ignored, and a manual change is never silently undone by the next
  recomputation. Rotation (`WinnerStays`, `RoundRobin`, `KingOfTheCourt`, `Manual`) is a per-session
  policy that shapes suggestions and moves nobody by itself. **A scoreline is never required to
  start the next match**: many clubs record nothing, and forcing entry would lose the board exactly
  the evenings it was built for.
- **Rest is a first-class matchmaking term, not a tie-break.** The complaint that actually ends
  evenings badly is unequal playing time, not unequal teams.
- **Matchmaking is deterministic and explainable by default.** Balance (`Rating` damped by
  `RatingDeviation`), rest, variety and a weak fit term, with a one-sentence reason attached to
  every proposal. The weightings are a per-community policy, because how much balance matters
  against how much variety is a fact about a group, not about the sport.
- **A language model is an optional strategy behind the same interface, off by default.** It exists
  to express what weights cannot — "keep the beginners spread out", "those two asked not to be
  paired". Its constraints: the operator supplies the provider (local or their own endpoint;
  Ssabba bundles and assumes none); it returns candidates that are then validated deterministically
  and discarded if invalid, never repaired; it cannot write a match, a player or a rating; it is off
  the critical path, so slow or absent means the arithmetic path runs; and where the endpoint is not
  on the instance, the settings screen and privacy notice say plainly that tonight's roster leaves
  the box. **Rejected:** the model as the primary path, and any hosted Ssabba matchmaking service.
- **Tournaments reuse the rows that exist.** The quick tournament is a bracket over whoever is
  checked in, running on the same board; the planned one is the same entities with registration and
  seeding. Bracket progression is pure domain code with a stored seed, so a draw is reproducible and
  checkable. Tournament matches count towards the ladder unless the community's `RuleSet` says
  otherwise. Cross-community tournaments stay blocked on issues #42 and #43 rather than growing a
  second mechanism.

## Consequences

- **The board is the first thing in Ssabba that needs real client-side state.** A beach has no
  signal; actions must queue and reconcile, and `Ssabba.Web.Client` finally earns the `area: client`
  label. This is a materially harder surface than anything built so far.
- **Two organisers on two phones can conflict.** Resolution is last-write-wins per court, with the
  board showing what changed rather than merging silently. This is a deliberate simplification and
  it will occasionally surprise somebody.
- **Proposal recomputation is combinatorial and must feel instant.** Thirty checked-in players
  across three free courts, recomputed every time a match ends. A good-enough heuristic that answers
  immediately beats an optimal one that does not, and the suggester should be written accordingly.
- **Unrated guest matches leak into statistics.** `PlayerFormatStat` counts matches that no rating
  reflects, so wins and rating will not reconcile for anyone who plays with visitors. Either the
  stats exclude them or the discrepancy is explained on the page; it cannot simply be ignored.
- **Guest rows are personal data about somebody with no account.** They cannot be reached by a
  `DataRequest`, cannot consent, and cannot be tagged in media. Keeping the record to a name and a
  session is what makes that tolerable.
- **An organiser can mark another adult a no-show.** That touches reliability and will occasionally
  be wrong or vindictive. It stays reversible and audited; there is no stronger answer on a
  single-community instance, for the same reason ADR-0003 could not find one for moderation.
- **A configurable model endpoint is a data egress path in a self-hosted app.** Operators will
  enable it without reading anything. The default must be off, the warning unmissable, and
  pseudonymisation before sending is an open question that should be settled before the feature
  ships.
- **`AttendanceState` grows to eight values**, several only meaningful during the session itself.
  Retrospective entry uses three of them and must not be made to look incomplete for it.
- **Open question:** whether a guest match should really be unrated for everybody, or rated for the
  rated players against an assumed provisional rating. The strict rule is chosen because it is
  honest and reversible; the loose one may be what clubs actually want.
- **Open question:** whether presence transitions deserve their own table or a compact audit trail
  on the existing `AuditEvent`. The trail is required either way.
