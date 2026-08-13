---
title: Safety and moderation
weight: 2
---

# Safety and moderation

A tracker that lets members tag each other in photos and bring strangers along as plus-ones needs
answers to "I do not want to be near that person" and "somebody should know about this". Until now
the only lever was `MembershipStatus.Suspended`: an admin action, inside one community, that the
person on the receiving end of the behaviour cannot reach.

## Blocking

A **block** is one player saying, about another player, *not me*. It is recorded between two
`Player` rows, not two memberships, and it applies across the whole instance — the harm it answers
to does not stop at a `CommunityId`.

A block suppresses the surfaces where the two would meet:

- neither sees the other's reactions;
- neither appears in the other's [discovery]({{< relref "discovery" >}}) results or partner search;
- neither can invite the other, or bring them as a plus-one;
- an organiser drawing teams is warned before putting them on the same court.

**A block never rewrites history.** It does not delete matches, does not alter `MatchAppearance`
rows, and does not move a rating. The ladder is a record of games that were actually played; if a
block could edit it, results would become falsifiable and the appearance trail that rating
recalculation depends on (issue #24) would develop holes. Two people who fell out last month still
played that match last year, and the ladder still says so.

Blocking is not moderation. It is a private preference, invisible to the community, and it asks
nothing of an organiser. When something needs to be *dealt with*, that is a report.

## Reporting

An `AbuseReport` names something that needs a human: conduct at a session, a photo somebody should
not have posted, a profile, a message in a session note. It carries a category, the details, and a
status that runs `Open → Triaged → Actioned` or `Rejected`, with `Withdrawn` for a reporter who
changes their mind — deliberately the same shape as `DisputeStatus`, so the queues read alike.

**A report about a person is not a dispute about a result.** `MatchDispute` already exists and stays
exactly as it is. "That score is wrong" and "that person frightened me" are different problems with
different urgencies and different readers; one queue for both serves neither.

Reports may be filed anonymously. Where they name a target that belongs to a community, the
community's organisers see them; where they do not, they land with the instance operator.

## Feedback

Feedback is the quieter channel: the thing you want the organiser to know that is not a report and
not an argument. Too many Tuesday sessions. The nets are always the wrong height. The dues are too
steep for students.

It can be filed **anonymously**, and anonymity here is not a gesture:

- the row stores **no author link at all** — a null `AuthorMemberId`, not a hidden one;
- **no `AuditEvent` is written** naming the actor, which is the ordinary behaviour everywhere else
  in the system and has to be suppressed deliberately here;
- consequently there is no path, for an operator with database access, from the entry back to a
  person.

A channel that promises anonymity and keeps a trail is worse than no channel, because it invites
candour it cannot honour. The cost is real and is accepted: an anonymous entry cannot be replied to,
cannot be verified, and is out of reach of a `DataRequest` erasure — there is no `PlayerId` to match
it against. That last point belongs in the instance's privacy notice.

## Where feedback goes

Feedback about **this club** stays on this instance. The operator reads and triages it; nothing is
transmitted anywhere. There is no in-app channel that sends anything to the Ssabba project, and
adding one would quietly turn every self-hosted instance into a reporting endpoint for somebody
else's server.

Feedback about **the software** — a bug, a missing feature, a security problem — goes to the
project directly, by hand. See [Support]({{< relref "../support" >}}).

## The community's own rules

Not every rule is a rule of volleyball. Clubs have views on who may bring guests, what happens to
the ball money, whether photographs get posted, how far in advance you may cancel, and how people
are expected to speak to each other. A `CommunityRuleDocument` is where a group writes that down: a
kind (code of conduct, house rules, safety, money), a body, and a version.

A document may require **acceptance**, recorded per member and per version, so that a rule which is
amended is genuinely re-agreed rather than silently changed underneath people. Acceptance is kept
apart from `ConsentRecord`, which answers a different question — consent is about what may be done
with your data, acceptance is about what you have agreed to do.

The sport's own rules are a separate matter entirely; see [Rules]({{< relref "../rules" >}}).
