---
title: Rules
weight: 10
---

# Rules and scoring

Three different things go by the name "rules" here, and they are kept apart:

- **[The official rules]({{< relref "official" >}})** of beach volleyball — external, published by
  FIVB, summarised and linked rather than reproduced.
- **[House rules]({{< relref "house-rules" >}})** — how a group deviates from them, as a `RuleSet`;
  and the group's own rules that have nothing to do with volleyball, as a `CommunityRuleDocument`.
- **The counting below** — how Ssabba turns sets into matches and matches into a ladder.

## Matches and sets

A match is a series of sets between two teams. Ssabba stores the points of every set rather than only
the winner, so statistics such as average point difference stay available.

The team that wins more sets wins the match. A match with an equal number of sets won stays
`Undecided` — that also covers matches still in progress.

## Ratings

Every player starts at **1000** points. After each match, ratings move by Elo with a K-factor of **24**:

```
expected  = 1 / (1 + 10^((opponentRating - rating) / 400))
newRating = rating + K * (actualScore - expected)
```

`actualScore` is 1 for a win, 0.5 for a draw and 0 for a loss. Beating a stronger opponent moves the
rating more than beating a weaker one, and the pair's gains and losses cancel out.

A rating belongs to a `CommunityMember`, not to a `Player`, and is meaningful only inside that
community — see [Concept]({{< relref "../concept" >}}). Where a level has to be shown to strangers,
Ssabba uses a self-declared band instead; see
[Discovery]({{< relref "../experience/discovery" >}}).
