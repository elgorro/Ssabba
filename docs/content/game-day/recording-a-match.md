---
title: Recording a match
weight: 5
---

# Recording a match

The set is over, somebody is already calling the next pairing, and the phone is in one hand. This is
the thirty seconds Ssabba has to be good at. Everything else in the app can afford a second look;
this cannot.

## What you enter

**The scores of the sets. That is all.** Who won is worked out from them, every time it is shown.
There is no winner field to disagree with the score, and no way to record a result whose winner is
not the side that took the sets.

The rest is filled in for you. The **format** is however many a side fielded — a pairing of two is
2v2, and the rating weight for that format follows. The **community** comes from the teams, the
**season** from whichever one is current, and the **scoring** — best of how many, to how many, by
how many — is copied down from the group's [house rules]({{< relref "../rules/house-rules" >}}) at
the moment you save, so amending them later never rewrites this result.

## The teams

Pick both from the list of lineups. If the pairing has never played before,
[form it first]({{< relref "../data-model/matches-and-ratings" >}}) — entering the same two people
again finds the team they already are rather than making a second one, in either order, so there is
no way to quietly split a pair's history in half.

Home and away carry no meaning on a beach. They are only labels for which column is which.

## When the score is refused

Ssabba checks that the score **could** have happened under the rules the match is being played
under, and says which set and why:

> Set 2 went past 21, so it must have been won by exactly 2, not 4.

That is a typo, not an argument. The commonest ones are a digit too many and two numbers the wrong
way round, and both produce a result that looks fine and moves somebody's rating.

If your group genuinely played something the check does not describe — one set straight to 15, first
to 21 with no margin, king of the court until the light went — the answer is not to fight the form.
It is to say so once, in the group's rule set: `WinBy` of zero switches the margin off, `SetsToWin`
of one makes a single set the whole match. Then every match after it is entered without complaint.
The full list of what is checked, and what deliberately is not, is in
[Rules and scoring]({{< relref "../rules" >}}).

Ssabba never checks whether a score is **true**. It cannot know. That is between the people who were
there.

## Fixing it afterwards

**Who may.** Organisers, admins and the owner may correct or strike any result — running matches is
the job. Everyone else may fix only a match they themselves played in, and only for a while
afterwards: **60 hours** by default, which covers a Friday evening through Monday morning. A group
that wants longer, shorter, or none at all sets its own window at `/community`; setting it to zero
leaves corrections to organisers. A single match can carry its own window, set when it is recorded.

Nobody else can touch it. Amending a result is not editing a row — it hands back the rating the match
took and applies the new one, so somebody outside it doing so would move four other people's ladder
positions.

Open the match and edit it. Correcting a score gives back the rating the old one moved and applies
the new one, so the ladder ends where entering it right the first time would have. Deleting hands the
rating back and strikes the match from the record — the row stays, so a rating that has already moved
can still be explained, but it stops counting.

Who played is not editable. A different pairing is a different match, and two other people's ratings
would have to move with it; record the right one instead.

One caveat worth knowing at the net: correcting a match is exact while nobody involved has played
since. Later matches were rated against the ratings this one set, so an old correction in the middle
of a busy season is close rather than perfect. Fix it the same evening and it is exact.

## Finding it again

Everything recorded is on **Matches**, most recent first, twenty-five to a page. The filters above
the table narrow it to one player, one lineup, a range of days, or any combination of those — a
player matches when they were in either team's lineup, and both ends of a date range are counted in
whole, so `1 June` to `2 June` includes a match played late on the second.

The filters live in the address, so the list you are looking at is the list you can send to somebody
else, and paging through it keeps them. Struck matches never appear, however you filter.
