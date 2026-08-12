---
title: Venues and sessions
weight: 2
---

# Venues and sessions

A session is a scheduled meet-up: a time, a court, a capacity and a list of who is coming.

```mermaid
erDiagram
    Venue ||--o{ Court : "courts"
    Court ||--o{ CourtReservation : "reservations"
    Court |o--o{ Session : "played on"
    Community ||--o{ SessionTemplate : "templates"
    Community ||--o{ Session : "sessions"
    SessionTemplate |o--o{ Session : "generated"
    Session ||--o{ SessionParticipant : "participants"
    Session ||--|| WeatherObservation : "weather"
    CommunityMember ||--o{ Session : "organises"
    CommunityMember ||--o{ SessionParticipant : "responses"

    Venue {
        Guid Id PK
        string Name
        string Address
        double Latitude
        double Longitude
        Guid OwnerCommunityId FK "nullable"
        VenueAccess Access
        string OpeningHours "JSON"
    }
    Court {
        Guid Id PK
        Guid VenueId FK,UK
        string Name UK
        CourtSurface Surface
        int NetHeightCm
        int MaxTeamSize
        bool IsActive
    }
    CourtReservation {
        Guid Id PK
        Guid CourtId FK
        Guid HeldByCommunityId FK
        Guid HeldByMemberId FK
        DateTimeOffset StartsAt
        DateTimeOffset EndsAt
        ReservationStatus Status
        long CostMinor
    }
    SessionTemplate {
        Guid Id PK
        Guid CommunityId FK
        Guid CourtId FK
        Guid DefaultRuleSetId FK
        string Rrule "RFC 5545"
        TimeOnly StartTimeLocal
        int DurationMinutes
        int GenerateAheadDays
    }
    Session {
        Guid Id PK
        Guid CommunityId FK
        Guid TemplateId FK
        Guid CourtId FK
        Guid RuleSetId FK
        Guid OrganizerMemberId FK
        DateTimeOffset StartsAt
        DateTimeOffset EndsAt
        int Capacity
        int MinPlayers
        SessionStatus Status
        long CostPerPlayerMinor
        DateTimeOffset DeletedAt "soft delete"
    }
    SessionParticipant {
        Guid Id PK
        Guid SessionId FK,UK
        Guid MemberId FK,UK
        Guid IsGuestOfMemberId FK "plus-one host"
        ParticipationResponse Response
        int WaitlistPosition "only when waitlisted"
        AttendanceState Attendance
    }
    WeatherObservation {
        Guid Id PK
        Guid SessionId FK,UK "one per session"
        string Provider
        double TemperatureC
        double WindKph
        double PrecipitationMm
        string ConditionText
    }
```

## Notes

**Courts cannot be double-booked.** `CourtReservation` carries a GiST exclusion constraint,
`EX_CourtReservations_NoOverlap`, which rejects any confirmed reservation whose
`tstzrange(StartsAt, EndsAt, '[)')` overlaps another on the same court. It only applies to
`Status = 0`, so cancelled reservations stay on record without blocking the slot. This needs the
`btree_gist` extension, created by the initial migration. A `CK_CourtReservations_EndsAfterStart`
check backs it up, and `Session` carries the equivalent `CK_Sessions_EndsAfterStart`.

**Recurring sessions** come from a `SessionTemplate` holding an RFC 5545 `Rrule`. Generated
occurrences are unique on `(TemplateId, StartsAt)` — a partial index, so ad-hoc sessions with no
template are unconstrained — which makes generation idempotent.

**The waitlist is explicit.** `WaitlistPosition` may only be set when `Response` is the waitlisted
value, enforced by `CK_SessionParticipants_WaitlistPosition`. `IsGuestOfMemberId` points at another
member of the same community and models plus-ones: the guest occupies a slot but the member who
brought them is recorded.

**Weather** is fetched once per session and cached; the unique `SessionId` makes it a genuine
zero-or-one relationship.

## Polls

Polls pick a date or settle a question. Votes are cast per option, so an approval-style poll is a
set of yes/no/if-need-be answers rather than a single choice.

```mermaid
erDiagram
    Community ||--o{ Poll : "polls"
    Poll ||--o{ PollOption : "options"
    PollOption ||--o{ Vote : "votes"
    CommunityMember ||--o{ Vote : "casts"
    CommunityMember ||--o{ Poll : "creates"
    Poll |o--|| Session : "result"
    Court |o--o{ PollOption : "proposed court"

    Poll {
        Guid Id PK
        Guid CommunityId FK
        Guid CreatedByMemberId FK
        Guid ResultSessionId FK "session it became"
        PollKind Kind
        string Question
        DateTimeOffset ClosesAt
        bool IsAnonymous
        bool AllowMultiple
        PollStatus Status
    }
    PollOption {
        Guid Id PK
        Guid PollId FK
        Guid CourtId FK
        string Label
        DateTimeOffset StartsAt
        DateTimeOffset EndsAt
        int SortOrder
    }
    Vote {
        Guid Id PK
        Guid PollOptionId FK,UK
        Guid MemberId FK,UK
        VoteValue Value "No, Yes, IfNeedBe"
        DateTimeOffset CastAt
    }
```

The unique `(PollOptionId, MemberId)` pair means one answer per member per option; changing a vote
updates the row rather than adding one. When a date poll resolves, `ResultSessionId` links the poll
to the session it produced.
