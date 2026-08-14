---
title: Matchmaking
weight: 4
---

# Matchmaking

Given the people currently `CheckedIn` and a free court, who plays whom? This is the question the
organiser answers thirty times an evening, and the one the app can genuinely help with.

## The default is arithmetic, and it explains itself

The suggester is deterministic. It scores candidate line-ups on four terms and offers the best few:

- **Balance.** The two sides should be close. `CommunityMember.Rating` drives this, softened by
  `RatingDeviation` — a newcomer's 1000 is a guess, and the suggester should not act as though it
  were measured.
- **Rest.** Whoever has waited longest plays next. In practice this dominates on a busy evening, and
  it should: see [Courts and rotation]({{< relref "courts" >}}).
- **Variety.** Playing with and against people you have not had tonight. Without this term, a
  balance-only suggester rebuilds the same four pairs all evening.
- **Fit.** Optional and weak: `PlayerProfile.PreferredPositions`, `HeightCm`, `SelfRatedLevel`, and
  — if and only if it exists and was consented to — the fitness [signal]({{< relref "signals" >}}).

**Every proposal carries its reason**, in one short sentence a human can argue with: *"closest match
of the four who have waited longest; Ana and Tom have not played together tonight."* An organiser
who cannot see why will not trust the board, and an unexplained suggestion that is merely usually
right is worse than a list sorted by waiting time, which at least nobody has to interrogate.

Proposals are advice. Pin a pair, swap two people, reject the lot — the next recomputation respects
what was pinned.

## The knobs belong to the community

How much balance matters against how much variety is not a fact about volleyball; it is a fact about
a group. A competitive Tuesday ladder and a mixed social Thursday want opposite answers, and neither
is wrong. A per-community matchmaking policy holds the weightings, whether guests may be put on the
same side, whether skill bands may be mixed, and how hard rest is enforced. It has defaults that
work untouched.

## The optional model

An operator may point Ssabba at a language model — one running on their own machine, or an endpoint
they chose — and write their preferences in prose rather than in weights:

> Keep the beginners spread out. Marek and Sofia asked not to be paired. If Lena is playing, she
> referees the last match. Nobody plays three in a row on court one.

That is the thing weights cannot express, and it is why the option exists. The constraints around it
are firm:

- **Off by default, and the app is complete without it.** No provider is bundled, none is assumed,
  and nothing degrades if none is configured.
- **The operator's model, the operator's choice.** A local model, a self-hosted endpoint, or a
  commercial API — Ssabba does not care which and does not privilege one. It is configuration, in
  the same category as the weather provider.
- **It suggests; it never writes.** The model returns candidate line-ups. Those go through the same
  deterministic validation as anything else: real people, currently `CheckedIn`, not already on a
  court, correct team size, blocks respected. A line-up that fails validation is discarded, not
  repaired. The model cannot create a player, record a match, or move a rating.
- **It is not on the critical path.** Slow, unreachable or nonsense — the arithmetic path is what
  runs, and the board does not wait.
- **It says plainly what leaves the instance.** Sending tonight's roster to an endpoint is sending
  personal data to that endpoint. Where the model is not on the instance, the operator must be told
  so in the settings screen and in the privacy notice, without euphemism. Names can be pseudonymised
  before they are sent; whether that should be the default is an open question.

**What it is not:** there is no chat, no assistant, and nothing generative anywhere else in Ssabba.
This is one optional strategy behind one interface, and the interface is what matters — the
arithmetic suggester and a model implement the same thing, and a third implementation could too.

Design is [ADR-0004]({{< relref "../adr/0004-game-day-orchestration" >}}).
