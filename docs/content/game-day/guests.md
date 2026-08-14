---
title: Guests
weight: 2
---

# Guests

Somebody who is not a member plays anyway. A member brings a friend, a visitor from another beach
walks up, a colleague is dragged along to see what the fuss is. This is ordinary, and an app that
cannot record it produces evenings whose match record is quietly wrong.

## Two different things called "guest"

The word already means something in Ssabba, and the two meanings must not merge.

| | What it is |
| --- | --- |
| `CommunityMember.Role = Guest` | **An account.** A real membership with limited rights, a `Rating`, and a place on the ladder. |
| A session guest | **No account.** A name on tonight's list, playing, holding no rating and appearing on no ladder. |

`SessionParticipant.IsGuestOfMemberId` already models the first half of the second case: a member
brings somebody and answers for them. What it does not carry is who that person is — the row still
belongs to the host member, so two friends brought by the same member cannot be told apart, and the
guest cannot be put on a court by name.

## What a guest is

A guest is a display name attached to one session, optionally with:

- the member who brought them and vouches for them,
- a rough skill hint so matchmaking has something to work with,
- their own presence state, exactly as in [Presence]({{< relref "presence" >}}) — a guest arrives,
  pauses and leaves like anybody else.

A guest can be put on a court and can appear in a recorded match. **A guest takes no rating.** They
have none to change, and their opponents' ratings must not move on the strength of a stranger nobody
can calibrate. A match containing a guest is recorded, is visible, counts in the session, and is
skipped by the rating calculation for every player in it — not just the guest — because a result
against an unrated player is not evidence about anybody. Whether that is too strict is a real
question the ladder work will have to settle.

## Becoming a member

Somebody who keeps coming stops being a guest. Promotion creates a `Player` and a `CommunityMember`
and links tonight's guest rows to it, so the history reads sensibly.

**Promotion does not retro-rate.** Their earlier matches stay unrated. Rewriting them would mean
recomputing every rating those opponents have earned since, on the strength of results that were
never rated evidence in the first place — see the recalculation trail in issue #24.

## The parts that are not comfortable

- **Anybody can be typed in.** A guest row is a name a member wrote, unverified. It is a note about
  the evening, not an identity, and nothing in the app should treat it as one.
- **A guest never consented to anything.** They did not accept the community's rules, they are not
  covered by any consent record, and they cannot be tagged in media. A guest name is personal data
  belonging to somebody who has no account with which to ask for it back, which is a reason to keep
  the record thin: a name, a session, nothing more.
- **Guests are invisible outside the session.** No profile, no ladder entry, no discovery, no
  cross-session history. A guest who came eleven times is eleven separate names until somebody
  promotes them.
