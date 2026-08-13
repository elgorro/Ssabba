---
title: House rules
weight: 2
---

# House rules

Nobody plays a full best-of-three to 21 when there are fourteen people waiting for two courts. Real
groups play one set to 15, or first to 21 straight, or king of the court until the light goes. A
tracker that insisted on the [official rules]({{< relref "official" >}}) would be a tracker people
enter fiction into.

## How a group deviates

A `RuleSet` is a community's house rules for one format: how many sets win a match, how many points
win a set, the margin, the deciding-set target, whether ends are swapped and how often, and whether
a let serve is played on. One rule set per community and format may be `IsDefault`, which is the one
picked when nobody chooses.

Two properties matter more than they look:

- **A played match copies the values down.** `Match` snapshots `SetsToWin`, `PointsPerSet`, `WinBy`
  and `TiebreakPoints` at the time it is recorded. Amending a rule set never rewrites a past result,
  so last season stays readable in the terms it was actually played under.
- **Format carries a rating weight.** `Format.RatingWeightPercent` lets a group decide that a
  scratch 4v4 moves the ladder less than a proper 2v2 does. See [Rules]({{< relref "." >}}) for the
  rating maths itself.

## Rules that are not about volleyball

Most of what a club argues about is not in any rulebook. Who may bring a guest. What happens to the
ball money when a session is rained off. Whether photographs get posted, and who may untag
themselves. How far ahead you have to cancel before it counts as a no-show — Ssabba does track that
one, as `AttendanceState.NoShow` and `ReliabilityScore`, but *how far ahead* is the group's call.
And, underneath all of it, how people are expected to speak to each other.

A `CommunityRuleDocument` is where a group writes those down: a kind — code of conduct, house rules,
safety, money — a body, and a version. A document may require **acceptance**, recorded per member
against a version, so amending it re-asks rather than changing the terms underneath people who
already agreed to something else.

This is separate from `ConsentRecord`, which answers a different question: consent governs what may
be done with your data, acceptance governs what you have agreed to do. See
[Safety and moderation]({{< relref "../experience/safety" >}}).
