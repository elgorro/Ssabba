namespace Ssabba.Domain.Entities;

/// <summary>The score of one set within a match.</summary>
public class MatchSet
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid MatchId { get; set; }
    public Match? Match { get; set; }

    /// <summary>1-based position of the set within the match.</summary>
    public int Number { get; set; }

    public int HomePoints { get; set; }

    public int AwayPoints { get; set; }
}
