namespace Ssabba.Domain.Entities;

/// <summary>
/// Marks that a player appears in a piece of media. This is the join that makes photo consent
/// actionable: without it, withdrawing consent has no way to find the pictures.
/// </summary>
public class MediaSubject
{
    public Guid MediaAssetId { get; set; }
    public MediaAsset? MediaAsset { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public DateTimeOffset TaggedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? TaggedByMemberId { get; set; }
    public CommunityMember? TaggedByMember { get; set; }
}
