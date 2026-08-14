---
title: Tournaments
weight: 5
---

# Tournaments

`Tournament`, `TournamentEntry` and the `TournamentId`, `TournamentRound` and `BracketSlot` columns
on `Match` have been in the schema since the start, and nothing reads them. `TournamentType` already
names the formats a beach club actually plays: round robin, single and double elimination, Swiss,
king of the court.

Two things called a tournament turn up in a club's life, and they want the same rows and completely
different screens.

## The quick one

Twenty-two people are checked in, the evening has an hour left, and somebody says "shall we just
play a tournament?" That is a decision with a thirty-second budget. Setup is: take whoever is
`CheckedIn`, pick round robin or a small bracket, let the app draw it, and start.

- Teams come from [matchmaking]({{< relref "matchmaking" >}}) or from whatever pairs already exist —
  including standing teams and ad-hoc ones.
- It runs on the same board as the rest of the evening
  ([Courts and rotation]({{< relref "courts" >}})); a bracket is a queue of matches with a shape,
  and the courts do not care why a match exists.
- Late arrivals and early leavers are the norm, not an exception. A quick tournament must survive
  somebody walking off in round two: the round completes, the pairing adapts, and nothing is voided.
- It is `Draft` to `Completed` inside ninety minutes and nobody ever looks at it again.

## The planned one

A weekend cup, entries opening a month ahead, seeding, a published bracket, possibly money. Here
`TournamentStatus` earns its `RegistrationOpen` state, `TournamentEntry` carries teams that signed
up rather than pairs assembled on the sand, and seeding matters because the draw is public before
anybody plays.

Seeding uses the community's own ratings where the entrants are members, and falls back to a
declared band otherwise — the same honesty problem
[Discovery]({{< relref "../experience/discovery" >}}) has, for the same reason.

## The bracket

Progression is the part with actual logic in it, and it is domain code with no I/O: given a
tournament type, an entry list and the results so far, produce the next round's fixtures. Draws must
be reproducible from a stored seed so that "the app randomised it" is a checkable claim rather than
an appeal to trust. Byes, walkovers and withdrawals are ordinary states, not edge cases — somebody
always fails to show for a cup.

## Tournaments and the ladder

**Tournament matches count towards the ladder by default.** They are matches; the sets were played.
A community that wants its cup kept out of the ladder says so through its `RuleSet`, which is
already how a group declares that a format deviates from the norm.

Matches containing a guest stay unrated, in a tournament as anywhere else — see
[Guests]({{< relref "guests" >}}).

## Across communities

An open cup drawing entries from three clubs is the obvious next want, and it is exactly the
`SharedTournaments` scope that `CommunityLink` already carries. It is also blocked: there is no
protocol (issue #42), and whose rating applies when players from two communities meet is undecided
(issue #43). Nothing here invents a second mechanism to get around that. Until federation exists, a
visitor from another club is a [guest]({{< relref "guests" >}}) or a member, like anybody else.

See [ADR-0002]({{< relref "../adr/0002-tenancy-and-federation" >}}).
