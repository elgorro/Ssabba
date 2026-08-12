namespace Ssabba.Domain.Entities;

/// <summary>
/// A question put to a community. Deliberately generic: finding a date is the common case, but the
/// same shape answers "which beach?" or "new net, yes or no?".
/// </summary>
public class Poll
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public PollKind Kind { get; set; } = PollKind.Date;

    public required string Question { get; set; }

    public Guid CreatedByMemberId { get; set; }
    public CommunityMember? CreatedByMember { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ClosesAt { get; set; }

    /// <summary>Hides who voted for what. Votes still record the member, so one person votes once.</summary>
    public bool IsAnonymous { get; set; }

    public bool AllowMultiple { get; set; } = true;

    public PollStatus Status { get; set; } = PollStatus.Open;

    /// <summary>The session a date poll produced, once someone acts on the result.</summary>
    public Guid? ResultSessionId { get; set; }
    public Session? ResultSession { get; set; }

    public ICollection<PollOption> Options { get; set; } = [];
}

public enum PollKind
{
    /// <summary>Options are candidate times; the winner becomes a session.</summary>
    Date = 0,

    /// <summary>Pick one of several.</summary>
    Choice = 1,

    /// <summary>Approve as many as you like.</summary>
    Approval = 2,
}

public enum PollStatus
{
    Open = 0,
    Closed = 1,
    Cancelled = 2,
}
