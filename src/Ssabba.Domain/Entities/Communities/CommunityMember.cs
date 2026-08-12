namespace Ssabba.Domain.Entities;

/// <summary>
/// A player's standing within one community. This is the pivot of the whole model: ratings, votes,
/// dues and permissions are all relative to a community, never global.
/// </summary>
public class CommunityMember
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    /// <summary>What this player is called here, when it differs from their display name.</summary>
    public string? Nickname { get; set; }

    public CommunityRole Role { get; set; } = CommunityRole.Member;

    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    /// <summary>Ladder rating within this community. A player rated here is not rated elsewhere.</summary>
    public int Rating { get; set; } = EloRating.InitialRating;

    /// <summary>Uncertainty around <see cref="Rating"/>; shrinks as matches accumulate.</summary>
    public int RatingDeviation { get; set; } = EloRating.InitialDeviation;

    public int MatchesPlayed { get; set; }

    /// <summary>Share of committed sessions actually attended, 0-100. Recomputed from attendance.</summary>
    public int ReliabilityScore { get; set; } = 100;

    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LeftAt { get; set; }
}

public enum CommunityRole
{
    /// <summary>Plays along, sees little, changes nothing.</summary>
    Guest = 0,

    Member = 1,

    /// <summary>May run sessions, polls and matches.</summary>
    Organizer = 2,

    /// <summary>May also manage members, money and equipment.</summary>
    Admin = 3,

    /// <summary>Admin who cannot be demoted or removed by other admins.</summary>
    Owner = 4,
}

public enum MembershipStatus
{
    /// <summary>Invited or requested, not yet accepted.</summary>
    Pending = 0,

    Active = 1,

    /// <summary>Temporarily barred; retains history and rating.</summary>
    Suspended = 2,

    Left = 3,
}
