---
title: "ADR-0005: Health signals from wearables"
weight: 5
---

# ADR-0005: Health signals from wearables

**Status:** proposed — 2026-08-14

## Context

Some players wear watches that measure a great deal about them, and
[ADR-0004]({{< relref "0004-game-day-orchestration" >}}) introduces a suggester that could use a
fatigue figure sensibly: somebody on their fifth match in a row is a different proposition from
somebody who has just arrived. The temptation is to ingest whatever the device offers and work out
later what to do with it.

That temptation should be resisted, for reasons that are not primarily about privacy law, though
they are that too. Physiological series are the most sensitive data Ssabba could hold. Ssabba is
self-hosted, typically by a club member on a small box, with backups nobody tests. The blast radius
of a heart rate archive on that box is out of all proportion to the benefit, which is a slightly
better guess about who should play next.

There is also a fairness problem. Most players will never have a watch, and a suggester that gets
meaningfully better for the people who do is a suggester that quietly sorts a club by who owns
hardware.

## Decision

- **Opt-in per player, defaulting to off**, recorded as a `ConsentRecord` like every other consent
  in the app. Never implied by connecting a device, and never a condition of anything.
- **Revocation deletes the stored series, not just a flag.** A consent that can only be withdrawn
  prospectively is a weaker promise than Ssabba should make, and the volume involved makes deletion
  cheap.
- **Store a coarse derived band and a timestamp, never the raw stream.** Something on the order of
  fresh / normal / tired. **Rejected: storing raw series** and deriving on demand. It would be more
  flexible and it is what every fitness product does; it is also an archive of medical-adjacent data
  sitting on a volunteer's server to support a feature that needs three values.
- **The band is private to the player.** Organisers do not see it, the group does not see it, it
  does not appear on a profile, and it is not exported. The suggester consumes it internally.
  **Rejected: showing the organiser who is tired**, which is the obvious "helpful" feature and turns
  the app into something people are monitored by.
- **The signal is one weak optional term.** Absence is a normal state, never a disadvantage, and no
  proposal is ever explained by somebody's lack of data.
- **Manual self-report is the primary source.** A three-point "how do you feel" needs no hardware,
  works for everybody, and is plausibly as useful. Device import is an addition to it, not the point
  of it.
- **No vendor is assumed or bundled.** Import goes through an interface the operator supplies an
  implementation for, exactly as the weather provider does. Ssabba does not become a client of any
  platform.
- **It is never a gate.** Nothing prevents somebody playing because of a number, and a rating moves
  on results only.

## Consequences

- **The feature is deliberately less useful than it could be.** A coarse band cannot support
  training insight, trend charts or injury warnings, and requests for all three will arrive. The
  answer is no, and this ADR is why.
- **Special-category data still lands in the database**, which brings the privacy notice, the
  consent record, backup handling and the deletion path with it. A small feature with a
  disproportionate compliance surface — worth knowing before it is built.
- **Deletion on revocation must actually reach backups' successors**, or the promise is only true of
  the live database. What is honestly achievable here should be stated in the privacy notice rather
  than overclaimed.
- **Self-reported and device-derived bands are not the same measurement** but share one field. Which
  produced a value should be recorded, or the suggester is mixing two instruments silently.
- **Open question:** whether the band should decay during an evening from matches played alone,
  giving every player the useful part of the feature with no device and no consent at all. If that
  proves good enough, most of this ADR becomes unnecessary, which would be the best outcome.
