namespace Ssabba.Domain.Entities;

/// <summary>
/// A lineup that played together, from a 2v2 pair up to a full side of six. Cheap and disposable:
/// most are made for a single match, so a team is not a club and carries no standing of its own.
/// </summary>
public class Team
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    /// <summary>Optional team name; when empty the UI falls back to the members' names.</summary>
    public string? Name { get; set; }

    /// <summary>Thrown together for one match, rather than a pairing that plays on.</summary>
    public bool IsAdHoc { get; set; } = true;

    /// <summary>
    /// The members' ids, sorted and joined — see <see cref="TeamRoster.Key"/>. Unique within the
    /// community, so the same lineup is only ever one row. Derived: rewrite it whenever the
    /// membership changes, or the next lookup will miss this team and make a duplicate.
    /// </summary>
    public string MemberKey { get; set; } = "";

    public ICollection<TeamMember> Members { get; set; } = [];
}

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public PlayingPosition Position { get; set; } = PlayingPosition.None;

    /// <summary>Order the members are listed in, so a team reads the same way every time.</summary>
    public int SortOrder { get; set; }
}
