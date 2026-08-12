namespace Ssabba.Domain.Entities;

/// <summary>
/// One member's answer on one option. The member is always recorded, even on an anonymous poll, so
/// that one person votes once; anonymity is enforced by never projecting the member outward.
/// </summary>
public class Vote
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid PollOptionId { get; set; }
    public PollOption? PollOption { get; set; }

    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public VoteValue Value { get; set; } = VoteValue.Yes;

    public DateTimeOffset CastAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum VoteValue
{
    No = 0,
    Yes = 1,

    /// <summary>Would rather not, but will if the alternative is not playing.</summary>
    IfNeedBe = 2,
}
