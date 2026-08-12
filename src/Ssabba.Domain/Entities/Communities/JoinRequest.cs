namespace Ssabba.Domain.Entities;

/// <summary>A player asking to join a community, the counterpart to <see cref="CommunityInvite"/>.</summary>
public class JoinRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public string? Message { get; set; }

    public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? DecidedByMemberId { get; set; }
    public CommunityMember? DecidedByMember { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }
}

public enum JoinRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Withdrawn = 3,
}
