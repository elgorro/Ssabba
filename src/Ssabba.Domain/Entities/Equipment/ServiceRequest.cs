namespace Ssabba.Domain.Entities;

/// <summary>
/// Something that needs doing: a torn net, a court that needs raking, sand that needs topping up.
/// Broader than equipment, because most of what goes wrong is the site rather than the kit.
/// </summary>
public class ServiceRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public ServiceRequestKind Kind { get; set; } = ServiceRequestKind.Other;

    public required string Subject { get; set; }

    public string? Description { get; set; }

    public Guid? EquipmentItemId { get; set; }
    public EquipmentItem? EquipmentItem { get; set; }

    public Guid? CourtId { get; set; }
    public Court? Court { get; set; }

    public Guid RaisedByMemberId { get; set; }
    public CommunityMember? RaisedByMember { get; set; }

    public Guid? AssignedToMemberId { get; set; }
    public CommunityMember? AssignedToMember { get; set; }

    public ServicePriority Priority { get; set; } = ServicePriority.Normal;

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Open;

    public DateTimeOffset RaisedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ResolvedAt { get; set; }

    /// <summary>What fixing it cost, if anything.</summary>
    public Guid? CostLedgerEntryId { get; set; }
    public LedgerEntry? CostLedgerEntry { get; set; }
}

public enum ServiceRequestKind
{
    EquipmentRepair = 0,
    CourtMaintenance = 1,
    SandTopUp = 2,
    Cleaning = 3,
    Purchase = 4,
    Other = 5,
}

public enum ServicePriority
{
    Low = 0,
    Normal = 1,
    High = 2,

    /// <summary>Unplayable or unsafe until it is dealt with.</summary>
    Blocking = 3,
}

public enum ServiceRequestStatus
{
    Open = 0,
    Acknowledged = 1,
    InProgress = 2,
    Resolved = 3,
    Rejected = 4,
}
