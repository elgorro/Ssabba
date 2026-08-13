---
title: Recognition
weight: 4
---

# Recognition

The ladder recognises exactly one thing: winning sets. Most of what a club actually values is
somewhere else — the person who brings the net every week, who taught half the group to serve, who
turned up in the rain in February, who has played five hundred matches.

A **badge** is how a community says so. It belongs to the community, it means what that group says
it means, and it is awarded to a `CommunityMember`.

## What a badge is not

- **Not points.** Badges do not add up, do not rank, and do not feed a score. There is no board of
  who has the most.
- **Not portable.** A badge does not travel to another community, for the same reason a rating does
  not: it is a statement by one group about one of its own.
- **Not automatic, to begin with.** The first pass awards badges by hand, by an organiser. Automatic
  criteria — five hundred matches, a season unbeaten — are deferred deliberately: it is easy to
  write a rule, and hard to withdraw a badge that a rule awarded to the wrong person.

## Shape

A `Badge` carries a name, a description, an emoji, whether it may be earned more than once, and
whether it is still active. Retiring a badge does not withdraw the ones already given.

A `BadgeAward` records who got it, who gave it, when, and an optional note — usually the interesting
part, since "for the net, every Tuesday, since 2019" says more than the badge does. An award can be
revoked, which keeps the record of it having happened rather than pretending it did not.

Badges are visible wherever the awarding community is visible, and no further.
