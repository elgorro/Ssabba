---
title: Social surfaces
weight: 1
---

# Social surfaces

## Reacting

A member can put an emoji on a match, a session, a photo or a poll. It is stored as a `Reaction`:
who reacted, what they reacted to, and which emoji. One member may attach several different emoji to
the same thing, but the same emoji only once — a unique index on
`(MemberId, TargetKind, TargetId, Emoji)` sees to that, so reacting twice is a toggle rather than a
tally.

**A "like" is not a separate thing.** It is the reaction carrying the default emoji. Building it as
its own entity would mean two mechanisms, two tables and two sets of rules for one gesture.

The set of available emoji is an allowlist, not the whole of Unicode. A short list keeps the surface
readable at a glance, keeps the column narrow, and avoids the moderation problem that arbitrary
emoji sequences create.

Reactions are read on the page of the thing they belong to. There is no aggregation, no ranking, and
no view of "everything that has been reacted to lately"; see
[Experience]({{< relref "." >}}) for why.

## Sharing

A `ShareLink` grants a **read-only view of exactly one match or session** to someone who has no
account — the friend who wants to see how Tuesday went, the visitor deciding whether to come along.

It is built the way `CommunityInvite` is built, and for the same reasons:

- The token is generated once and only its **hash** is stored, so the database never holds a working
  key.
- It **expires**, and it can be **revoked** at any time by whoever made it or by an organiser.
- It carries a view count, so a link that is being passed around more widely than intended is
  visible rather than silent.

What a share link deliberately does not expose: contact details, the ledger, attendance beyond who
played, other sessions, the ladder, or any page other than the one it points at. A share link is a
window, not a guest account.

## Public and private

`Audience` says who may see a team, a match or a session:

| Value | Who sees it |
| --- | --- |
| `Private` | The people involved, and the community's organisers. |
| `Members` | Anyone in the owning community. The normal setting. |
| `Linked` | Also communities linked to this one. **Inert** — nothing consumes it until federation exists (issue #42). |
| `Public` | Anyone, including people with no account. Required for a session to appear in [Discovery]({{< relref "discovery" >}}). |

The default is `Members`. A club that plays on a public beach and wants strangers to join can move
its sessions to `Public`; a club that does not, never has to think about the setting.

`Audience` is separate from the community's own `CommunityVisibility`, which governs whether the
community itself is listed. A public session inside an unlisted community is a sensible combination:
come and play, but you are not joining anything.

## Teams that persist

Today a `Team` is disposable — `IsAdHoc` defaults to true, most teams are made for one match, and a
team carries no standing of its own. That is right for a Tuesday round where the pairs are drawn out
of a hat.

It is wrong for the pair that has played together for three seasons. A **standing team** is a named
lineup that persists across matches, has members who joined it deliberately rather than by being
listed on a scoresheet, and has an `Audience` of its own. It still carries no rating: ratings belong
to `CommunityMember`, and a team rating would be a second, conflicting answer to "how good is this
person".
