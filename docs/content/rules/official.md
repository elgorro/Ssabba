---
title: Official rules
weight: 1
---

# The official rules

Beach volleyball has a governing body and a published rulebook. Ssabba does not reproduce it, and
this page is a **summary with citations**, not a substitute — the rulebook is copyrighted, it is
revised on a cycle, and a stale copy inside a wiki is worse than a link.

- **FIVB Official Beach Volleyball Rules** — the authority, published by the Fédération
  Internationale de Volleyball at [fivb.com](https://www.fivb.com/) and issued per rule cycle.
- Your national federation publishes its own edition, usually a translation with local amendments
  for youth and amateur play. For a club, that edition is the one that matters.

## What Ssabba assumes

These are the defaults the schema is built around. Each maps onto a field, so a group that plays
differently changes a number rather than arguing with the software.

| Rule | Official (2v2) | Where it lives |
| --- | --- | --- |
| Court | 16 × 8 m, no attack line | Not modelled; `Court` records surface and net height only |
| Net height | 2.43 m men, 2.24 m women | `Court.NetHeightCm` |
| Match | Best of three sets | `RuleSet.SetsToWin` = 2 |
| Set | To 21, rally scoring | `RuleSet.PointsPerSet` = 21 |
| Winning margin | Two clear points, no ceiling | `RuleSet.WinBy` = 2 |
| Deciding set | To 15, still two clear | `RuleSet.TiebreakPoints` = 15 |
| Switching ends | Every 7 points, every 5 in the deciding set | `RuleSet.SwitchEveryPoints` |
| Touches | Three per side; a block does not count as one | Not modelled — Ssabba records outcomes, not rallies |
| Serve off the net | A let serve is played on | `RuleSet.LetServeAllowed` |

These are not only stored, they are **enforced on entry**. A recorded match keeps its own copy of
`SetsToWin`, `PointsPerSet`, `WinBy` and `TiebreakPoints`, and a score is checked against that copy
before it is accepted — so a set that could not have ended the way it was typed is refused, in the
terms the group plays under rather than these. See
[what counts as a plausible score]({{< relref "." >}}).

Indoor volleyball differs on most of these — 25-point sets, five sets, a block that does not count
as a touch either, a libero, rotation. `Format` distinguishes 2v2 through 6v6 and carries its own
defaults, so a club that plays both is not forced to pick one.

## What Ssabba does not know

Ssabba records **results**, not play. It has no notion of a rally, a rotation, a fault, a sanction,
a time-out or a referee's decision. There is no scoreboard mode and no officiating aid. The set
score is entered after the fact by a person who was there, and the model goes no deeper than that
deliberately: the alternative is a rules engine that has to be right about every edition of every
federation's amendments, and is worth less than the paper the group already keeps score on.

A disputed result is handled socially rather than by rule: see `MatchDispute` and
[Matches and ratings]({{< relref "../data-model/matches-and-ratings" >}}).

## Deviating

Almost every group deviates from the official rules, and none of them are wrong to. See
[House rules]({{< relref "house-rules" >}}).
