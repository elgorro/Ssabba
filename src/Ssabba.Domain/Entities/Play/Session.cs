namespace Ssabba.Domain.Entities;

/// <summary>
/// A gathering to play: one evening at the court. Attendance, matches, costs, equipment and the
/// weather all hang off this rather than off individual matches.
/// </summary>
public class Session
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    /// <summary>Set when generated from a recurring fixture.</summary>
    public Guid? TemplateId { get; set; }
    public SessionTemplate? Template { get; set; }

    public required string Title { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public Guid? CourtId { get; set; }
    public Court? Court { get; set; }

    /// <summary>Places available; beyond this, responses land on the waiting list.</summary>
    public int? Capacity { get; set; }

    /// <summary>Below this many confirmed players the session is not worth holding.</summary>
    public int? MinPlayers { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Draft;

    public Guid? RuleSetId { get; set; }
    public RuleSet? RuleSet { get; set; }

    /// <summary>What each player owes for turning up, in minor units of the community's currency.</summary>
    public long CostPerPlayerMinor { get; set; }

    public Guid OrganizerMemberId { get; set; }
    public CommunityMember? OrganizerMember { get; set; }

    public string? Notes { get; set; }

    public string? CancellationReason { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<SessionParticipant> Participants { get; set; } = [];
}

public enum SessionStatus
{
    /// <summary>Being prepared; not yet visible to members.</summary>
    Draft = 0,

    /// <summary>Published and taking responses.</summary>
    Open = 1,

    /// <summary>Going ahead: enough players said yes.</summary>
    Confirmed = 2,

    Cancelled = 3,

    /// <summary>Played and closed for attendance changes.</summary>
    Completed = 4,
}
