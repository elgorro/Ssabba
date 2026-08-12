namespace Ssabba.Domain.Entities;

/// <summary>
/// One player's participation in one match, with what it did to their rating. Teams are lineups and
/// change; who actually played a given match does not. This is therefore the record ratings are
/// computed from and replayed against, not <see cref="TeamMember"/>.
/// </summary>
public class MatchAppearance
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid MatchId { get; set; }
    public Match? Match { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    /// <summary>The membership whose rating this appearance moved.</summary>
    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public MatchSide Side { get; set; }

    /// <summary>Came on partway through; still counts, but flagged for the record.</summary>
    public bool IsSubstitute { get; set; }

    public int RatingBefore { get; set; }

    public int RatingAfter { get; set; }

    /// <summary>Stored rather than derived, so a rounding rule change cannot rewrite history.</summary>
    public int RatingDelta { get; set; }
}

public enum MatchSide
{
    Home = 0,
    Away = 1,
}
