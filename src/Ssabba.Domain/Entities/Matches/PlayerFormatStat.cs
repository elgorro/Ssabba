namespace Ssabba.Domain.Entities;

/// <summary>
/// A member's record in one format, optionally within one season. Purely derived: every value can
/// be rebuilt from <see cref="MatchAppearance"/>, and it exists so the ladder is one read.
/// </summary>
public class PlayerFormatStat
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public Guid FormatId { get; set; }
    public Format? Format { get; set; }

    /// <summary>Null holds the all-time figures, alongside one row per season.</summary>
    public Guid? SeasonId { get; set; }
    public Season? Season { get; set; }

    public int Matches { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int SetsWon { get; set; }

    public int SetsLost { get; set; }

    public int PointsFor { get; set; }

    public int PointsAgainst { get; set; }

    public DateTimeOffset? LastPlayedAt { get; set; }
}
