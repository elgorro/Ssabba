namespace Ssabba.Domain.Entities;

/// <summary>A pairing of players. Beach volleyball is usually 2v2, but the model allows other sizes.</summary>
public class Team
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>Optional team name; when empty the UI falls back to the members' names.</summary>
    public string? Name { get; set; }

    public ICollection<TeamMember> Members { get; set; } = [];
}

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }
}
