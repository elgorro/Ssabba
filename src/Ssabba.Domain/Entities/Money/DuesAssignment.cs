namespace Ssabba.Domain.Entities;

/// <summary>One member's liability under a dues plan, and whether it has been settled.</summary>
public class DuesAssignment
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid DuesPlanId { get; set; }
    public DuesPlan? DuesPlan { get; set; }

    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public DateOnly DueOn { get; set; }

    /// <summary>Copied from the plan, so raising the fee does not restate what was already owed.</summary>
    public long AmountMinor { get; set; }

    public DuesStatus Status { get; set; } = DuesStatus.Pending;

    /// <summary>The ledger movement that settled this.</summary>
    public Guid? PaidLedgerEntryId { get; set; }
    public LedgerEntry? PaidLedgerEntry { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public string? WaivedReason { get; set; }
}

public enum DuesStatus
{
    Pending = 0,
    Paid = 1,

    /// <summary>Excused, e.g. hardship or a long injury.</summary>
    Waived = 2,

    Overdue = 3,
}
