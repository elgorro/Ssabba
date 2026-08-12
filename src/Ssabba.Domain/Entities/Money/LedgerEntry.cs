namespace Ssabba.Domain.Entities;

/// <summary>
/// One movement of money, debited from one account and credited to another. Double entry from the
/// start, because dues, session fees, expenses and sponsorship are then one mechanism rather than
/// four, and the books always balance by construction.
/// </summary>
/// <remarks>
/// Amounts are whole minor units — cents, not euros — so nothing is ever lost to binary fractions.
/// </remarks>
public class LedgerEntry
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public required string Description { get; set; }

    /// <summary>Where the money went to.</summary>
    public Guid DebitAccountId { get; set; }
    public Account? DebitAccount { get; set; }

    /// <summary>Where the money came from.</summary>
    public Guid CreditAccountId { get; set; }
    public Account? CreditAccount { get; set; }

    /// <summary>Always positive: direction is carried by the two accounts, not by the sign.</summary>
    public long AmountMinor { get; set; }

    public required string Currency { get; set; }

    public LedgerCategory Category { get; set; } = LedgerCategory.Other;

    public Guid? SessionId { get; set; }
    public Session? Session { get; set; }

    public Guid? EquipmentItemId { get; set; }
    public EquipmentItem? EquipmentItem { get; set; }

    public Guid? ServiceRequestId { get; set; }
    public ServiceRequest? ServiceRequest { get; set; }

    public Guid? FundingSourceId { get; set; }
    public FundingSource? FundingSource { get; set; }

    public Guid? CreatedByMemberId { get; set; }
    public CommunityMember? CreatedByMember { get; set; }

    public Guid? ReceiptMediaId { get; set; }
    public MediaAsset? ReceiptMedia { get; set; }

    /// <summary>The entry this one reverses. Corrections are booked, never edited away.</summary>
    public Guid? ReversesEntryId { get; set; }
    public LedgerEntry? ReversesEntry { get; set; }
}

public enum LedgerCategory
{
    Dues = 0,
    SessionFee = 1,
    CourtRental = 2,
    Equipment = 3,
    Maintenance = 4,
    Sponsorship = 5,
    Donation = 6,
    Grant = 7,
    Refund = 8,
    Other = 9,
}
