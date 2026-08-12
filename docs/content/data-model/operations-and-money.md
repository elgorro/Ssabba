---
title: Operations and money
weight: 4
---

# Operations and money

Communities buy balls, rent courts and collect dues. Ssabba books all of it through a double-entry
ledger so a balance is always the sum of entries rather than a number someone edited.

```mermaid
erDiagram
    Community ||--o{ Account : "accounts"
    CommunityMember |o--|| Account : "member balance"
    Account ||--o{ LedgerEntry : "debit"
    Account ||--o{ LedgerEntry : "credit"
    LedgerEntry |o--o{ LedgerEntry : "reverses"
    Community ||--o{ DuesPlan : "dues plans"
    DuesPlan ||--o{ DuesAssignment : "assignments"
    CommunityMember ||--o{ DuesAssignment : "owes"
    DuesAssignment |o--|| LedgerEntry : "paid by"
    Community ||--o{ FundingSource : "funding"
    Season |o--o{ DuesPlan : "season"

    Account {
        Guid Id PK
        Guid CommunityId FK
        Guid MemberId FK,UK "set iff MemberBalance"
        string Name
        AccountKind Kind "Cash, Bank, MemberBalance, Sponsorship, Expense"
        char Currency
        bool IsActive
    }
    LedgerEntry {
        Guid Id PK
        Guid CommunityId FK
        Guid DebitAccountId FK
        Guid CreditAccountId FK
        Guid SessionId FK
        Guid EquipmentItemId FK
        Guid ServiceRequestId FK
        Guid FundingSourceId FK
        Guid ReceiptMediaId FK
        Guid ReversesEntryId FK
        DateTimeOffset OccurredAt
        long AmountMinor "always positive"
        char Currency
        LedgerCategory Category
    }
    DuesPlan {
        Guid Id PK
        Guid CommunityId FK
        Guid SeasonId FK
        string Name
        long AmountMinor
        DuesPeriod Period
        CommunityRole AppliesToRole "nullable"
        bool IsActive
    }
    DuesAssignment {
        Guid Id PK
        Guid DuesPlanId FK,UK
        Guid MemberId FK,UK
        DateOnly DueOn UK
        long AmountMinor "copied from the plan"
        DuesStatus Status
        Guid PaidLedgerEntryId FK
    }
    FundingSource {
        Guid Id PK
        Guid CommunityId FK
        Guid ContactPlayerId FK
        Guid LogoMediaId FK
        string Name
        FundingKind Kind
        long AmountMinor
        FundingStatus Status
        DateOnly StartsOn
    }
```

## Notes

**Amounts are always positive** (`CK_LedgerEntries_AmountPositive`) and direction is carried by
which account is debited and which credited — the two must differ
(`CK_LedgerEntries_DistinctAccounts`). Corrections are made by booking a reversing entry that points
at the original through `ReversesEntryId`; nothing is edited or deleted. Both account references are
`Restrict`: an account with entries against it cannot be removed out from under the books.

**A member's balance is an account.** `CK_Accounts_MemberBalanceHasMember` enforces the biconditional
`(Kind = MemberBalance) = (MemberId IS NOT NULL)`, and a partial unique index gives each member at
most one.

**Optional context, never required.** `SessionId`, `EquipmentItemId`, `ServiceRequestId`,
`FundingSourceId` and `ReceiptMediaId` are all `SetNull`. Deleting a session must not delete the
money booked against it — the entry simply loses its context.

**Dues are generated, then tracked.** A `DuesPlan` describes what is owed and how often; a
`DuesAssignment` is one concrete instalment for one member, unique on
`(DuesPlanId, MemberId, DueOn)` so regeneration is safe. It copies `AmountMinor` from the plan for
the same reason a match copies its scoring rules, and links to the `LedgerEntry` that settled it.

## Equipment and service

```mermaid
erDiagram
    Community ||--o{ EquipmentItem : "equipment"
    Venue |o--o{ EquipmentItem : "kept at"
    EquipmentItem ||--o{ EquipmentLoan : "loans"
    CommunityMember ||--o{ EquipmentLoan : "borrows"
    Session |o--o{ EquipmentLoan : "for session"
    Community ||--o{ ServiceRequest : "requests"
    EquipmentItem |o--o{ ServiceRequest : "about"
    Court |o--o{ ServiceRequest : "about"
    CommunityMember ||--o{ ServiceRequest : "raises / assigned"
    ServiceRequest |o--|| LedgerEntry : "cost"

    EquipmentItem {
        Guid Id PK
        Guid CommunityId FK
        Guid HomeVenueId FK
        string Name
        EquipmentKind Kind
        string AssetTag UK "unique per community"
        long PurchasePriceMinor
        EquipmentCondition Condition
        EquipmentStatus Status
    }
    EquipmentLoan {
        Guid Id PK
        Guid EquipmentItemId FK,UK
        Guid MemberId FK
        Guid SessionId FK
        DateTimeOffset CheckedOutAt
        DateTimeOffset DueBackAt
        DateTimeOffset ReturnedAt "null while out"
        EquipmentCondition ConditionOut
        EquipmentCondition ConditionIn
    }
    ServiceRequest {
        Guid Id PK
        Guid CommunityId FK
        Guid EquipmentItemId FK
        Guid CourtId FK
        Guid RaisedByMemberId FK
        Guid AssignedToMemberId FK
        Guid CostLedgerEntryId FK
        ServiceRequestKind Kind
        string Subject
        ServicePriority Priority
        ServiceRequestStatus Status
    }
```

An item can only be out once: a partial unique index on `EquipmentItemId WHERE ReturnedAt IS NULL`
makes a second checkout impossible until the first is returned, and
`CK_EquipmentLoans_ReturnedAfterCheckout` keeps the dates sane. Recording condition on the way out
and on the way back is what turns a loan into evidence when something comes back broken —
`ServiceRequest` then carries the repair, and its cost points at the ledger entry that paid for it.
