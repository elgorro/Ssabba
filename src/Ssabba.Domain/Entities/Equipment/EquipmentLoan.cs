namespace Ssabba.Domain.Entities;

/// <summary>Who has the kit, since when, and what state it went out and came back in.</summary>
public class EquipmentLoan
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid EquipmentItemId { get; set; }
    public EquipmentItem? EquipmentItem { get; set; }

    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public DateTimeOffset CheckedOutAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DueBackAt { get; set; }

    /// <summary>Null while the item is still out.</summary>
    public DateTimeOffset? ReturnedAt { get; set; }

    /// <summary>The session it was taken for, when it was taken for one.</summary>
    public Guid? SessionId { get; set; }
    public Session? Session { get; set; }

    public EquipmentCondition ConditionOut { get; set; }

    public EquipmentCondition? ConditionIn { get; set; }

    public string? Note { get; set; }
}
