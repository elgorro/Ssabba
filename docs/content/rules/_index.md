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
`Undecided` — that also covers matches still in progress. The winner is never entered by hand: it is
read off the sets every time it is shown, and there is no field for it to disagree with.

A result is only recorded once one side has taken the sets it needed. Half a match is a real thing to
be in the middle of, but not a thing to be rated on.

### What counts as a plausible score

The commonest thing to happen at the net is a typo, and the commonest typo — a digit too many, or two
numbers the wrong way round — produces a result that is quietly wrong rather than obviously wrong,
and then moves somebody's rating. So a score is checked before it is accepted.

Every rule is read from **the match's own snapshot**, not from the official rules and not from the
rule set as it stands today. A group that plays one set to 15 is never told it is wrong; a group that
plays to 21 by two is told when it enters something that cannot have happened:

- **No set is level.** A set has a winner, or it was not played.
- **The winner reached the target** — `PointsPerSet`, or `TiebreakPoints` in the deciding set when
  the group plays a shorter one.
- **The margin is at least `WinBy`**, and a set that went *past* the target ends on exactly `WinBy`.
  A set to 21 by two cannot finish 25–21: it was over at 23–21.
- **The match is no longer than `2 × SetsToWin - 1` sets**, and no set is recorded after one side had
  already won. A decided match does not carry on.

The escape hatch is the rule set itself. A `WinBy` of one or zero switches the margin rule off
entirely, which is what a group playing straight to a number wants, and `SetsToWin` of one makes a
single set the whole match. Nobody has to argue with the software to record what they played — see
[House rules]({{< relref "house-rules" >}}).

What is deliberately *not* checked is whether the score is **true**. Ssabba can tell that 25–21 never
happened; it has no way of knowing whether 21–18 did. Only the people who were there know that, and a
[dispute]({{< relref "../data-model/matches-and-ratings" >}}) is settled between them rather than by
the software.

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

### Correcting a result

A score entered wrongly can be fixed, and a match entered twice can be struck from the record.
Either way the rating goes back: every `MatchAppearance` stores the change it made
(`RatingBefore`, `RatingAfter`, `RatingDelta`) precisely so it can be handed back without being
recomputed. Editing then applies the corrected score, leaving the ladder where entering it right the
first time would have. Deleting hands it back for good.

A deleted match is **struck, not erased**. The row stays, marked `Voided`, and drops out of every
list — because a result that vanishes cannot explain a rating that has already moved.

One honest limit: this is exact while nobody involved has played since. Elo is path-dependent, so a
later match rated against a rating that this one set would itself need re-rating. Replaying a
community's whole history is a separate piece of work, and until it lands, correcting an old match in
a busy season is an approximation rather than a rewind.
