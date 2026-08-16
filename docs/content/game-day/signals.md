---
title: Signals
weight: 7
---

# Signals

Some players wear a watch. Most do not, and never will. This page describes what Ssabba does with
the ones that exist, and — more importantly — what it refuses to do.

## What it is for

Two narrow uses, both advisory:

- **A weak term in team suggestions.** Somebody who has already played four matches back to back is
  a different proposition from somebody who has just arrived, and a readiness figure knows that
  better than a waiting-time counter does.
- **A nudge to the person themselves.** "That is your fifth in a row" — shown to them, not to the
  group.

That is the whole ambition. It is a small improvement to [matchmaking]({{< relref "matchmaking" >}})
and nothing else.

## Consent, and what revocation means

Health data is special-category personal data, and the rules around it are not optional politeness.

- **Per-player opt-in**, defaulting to off, asked for in plain words, recorded as a
  `ConsentRecord` like every other consent in the app.
- **Revocable at any time, and revocation deletes.** Turning the signal off removes the stored
  series, not just a flag. A consent you can withdraw only prospectively is a weaker promise than
  the one Ssabba should make.
- **No coercion.** A player without a watch, or without consent, is never disadvantaged: the signal
  is one small optional term, absence is a normal state, and no proposal is ever explained as
  "because she has no data".

## Coarse on purpose

Ssabba stores a **derived band and a timestamp** — something like fresh, normal or tired, computed
from whatever the source gave — and not the raw stream. No heart rate series, no sleep record, no
recovery score history, no route.

This is a deliberate refusal, not a limitation waiting to be lifted. Raw physiological series are
the most sensitive data an app of this kind could hold; a beach volleyball tracker has no business
holding a heart rate archive on a self-hosted box maintained by a volunteer. A band is enough for
the only two uses above, and it is dramatically cheaper to protect.

**Nobody else sees it.** The player sees their own band; the organiser does not, and the group
certainly does not. The suggester consumes it internally. There is no leaderboard of fitness, no
comparison, and no export.

## Getting the data in

- **Manual** first: the player says how they feel, on a three-point scale. This is the baseline, it
  needs no vendor, and it may well be as useful as the hardware.
- **A provider the operator configures**, for those who have one. No vendor is assumed, none is
  bundled, and the model is the same as the weather provider: an interface with an implementation
  the operator supplies. Ssabba does not become a Garmin client, or an Apple one.

## What it will never be

- Not a health record. Not a training log. Not an injury tracker.
- Not a gate: nothing stops somebody playing because a number said so. That decision belongs to the
  person on the sand.
- Not evidence. A rating moves on results, never on physiology.

Design and the rejected alternatives are in
[ADR-0005]({{< relref "../adr/0005-health-signals" >}}).
