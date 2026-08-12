---
title: Rules
weight: 10
---

# Rules and scoring

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
