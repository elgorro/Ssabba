namespace Ssabba.Domain.Entities;

/// <summary>A challenge to a recorded result, raised before or after it counted.</summary>
public class MatchDispute
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid MatchId { get; set; }
    public Match? Match { get; set; }

    public Guid RaisedByMemberId { get; set; }
    public CommunityMember? RaisedByMember { get; set; }

    public required string Reason { get; set; }

    public DisputeStatus Status { get; set; } = DisputeStatus.Open;

    public DateTimeOffset RaisedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? ResolvedByMemberId { get; set; }
    public CommunityMember? ResolvedByMember { get; set; }

    public string? Resolution { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}

public enum DisputeStatus
{
    Open = 0,

    /// <summary>The result was changed.</summary>
    Upheld = 1,

    /// <summary>The result stood.</summary>
    Rejected = 2,

    Withdrawn = 3,
}
