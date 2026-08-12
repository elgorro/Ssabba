---
title: Privacy and notifications
weight: 5
---

# Privacy and notifications

```mermaid
erDiagram
    Community |o--o{ MediaAsset : "media"
    CommunityMember |o--o{ MediaAsset : "uploaded"
    MediaAsset ||--o{ MediaSubject : "subjects"
    Player ||--o{ MediaSubject : "tagged in"
    Player ||--o{ ConsentRecord : "consents"
    Community |o--o{ ConsentRecord : "scope"
    Player ||--o{ DataRequest : "requests"
    DataRequest |o--|| MediaAsset : "result file"
    Community |o--o{ AuditEvent : "audit"
    Player |o--o{ AuditEvent : "actor"

    MediaAsset {
        Guid Id PK
        Guid CommunityId FK "null = instance-wide"
        Guid UploadedByMemberId FK
        string StoragePath
        string ContentType
        long Bytes
        ContactVisibility Visibility
        string Sha256 "dedupe"
        DateTimeOffset DeletedAt "soft delete"
    }
    MediaSubject {
        Guid MediaAssetId PK,FK
        Guid PlayerId PK,FK
        Guid TaggedByMemberId FK
        DateTimeOffset TaggedAt
    }
    ConsentRecord {
        Guid Id PK
        Guid PlayerId FK
        Guid CommunityId FK "null = instance-wide"
        ConsentKind Kind
        bool Granted
        DateTimeOffset RecordedAt
        string PolicyVersion
        string Source
    }
    DataRequest {
        Guid Id PK
        Guid PlayerId FK
        Guid ResultMediaId FK
        DataRequestKind Kind "Export or Erasure"
        DataRequestStatus Status
        DateTimeOffset RequestedAt
        DateTimeOffset CompletedAt
    }
    AuditEvent {
        Guid Id PK
        Guid CommunityId FK
        Guid ActorPlayerId FK
        string Action
        string EntityType
        Guid EntityId "no foreign key"
        DateTimeOffset OccurredAt
        string Data "JSON"
        string IpHash
    }
```

## Notes

**Consent is append-only.** A `ConsentRecord` is never updated; withdrawing consent writes a new row
with `Granted = false`. The current state is the newest row for that player and kind, which the
index `(PlayerId, Kind, RecordedAt)` serves. `PolicyVersion` records which text was agreed to, so a
policy change can be detected rather than assumed.

**`AuditEvent.EntityId` deliberately has no foreign key.** The audit log has to outlive the rows it
describes — otherwise deleting a record would erase the evidence that it was deleted. `EntityType`
plus `EntityId` is a loose reference, indexed as a pair. `IpHash` stores a hash, never an address.

**Media is deduplicated by content.** `Sha256` is indexed where not null, so the same photo uploaded
twice can be stored once. Tagging a person in a photo (`MediaSubject`) is a composite-key join, and
it is what a data export or erasure request has to walk to find every image someone appears in.

## Notifications

```mermaid
erDiagram
    CommunityMember ||--o{ NotificationPreference : "preferences"
    Player ||--o{ NotificationOutbox : "recipient"
    Community |o--o{ NotificationOutbox : "scope"

    NotificationPreference {
        Guid Id PK
        Guid MemberId FK,UK
        NotificationKind Kind UK
        NotificationChannel Channels "flags"
        int LeadTimeMinutes
    }
    NotificationOutbox {
        Guid Id PK
        Guid CommunityId FK
        Guid RecipientPlayerId FK
        NotificationKind Kind
        NotificationChannel Channel
        string Payload "JSON"
        DateTimeOffset ScheduledFor
        DateTimeOffset SentAt
        int Attempts
        string LastError
        DateTimeOffset AbandonedAt
    }
```

`NotificationOutbox` is a transactional outbox: a notification is written in the same transaction as
the change that caused it, and a sender picks it up afterwards. The partial index
`IX_NotificationOutbox_Pending` on `ScheduledFor WHERE SentAt IS NULL AND AbandonedAt IS NULL` keeps
that poll cheap no matter how much history accumulates. Repeated failures raise `Attempts` and
record `LastError`; `AbandonedAt` retires a message that will never send.

Preferences are per membership and per kind, with `Channels` a flags enum, so one row says "remind me
about sessions by email and push, two hours ahead".
