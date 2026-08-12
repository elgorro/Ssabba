namespace Ssabba.Domain.Entities;

/// <summary>A single game between two teams, made up of one or more sets.</summary>
public class Match
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset PlayedAt { get; set; }

    public string? Location { get; set; }

    public Guid HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }

    public Guid AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }

    public Guid? TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    public ICollection<MatchSet> Sets { get; set; } = [];

    public int HomeSetsWon => Sets.Count(s => s.HomePoints > s.AwayPoints);

    public int AwaySetsWon => Sets.Count(s => s.AwayPoints > s.HomePoints);

    /// <summary>Which side won, or <c>null</c> while the match is still tied or unfinished.</summary>
    public MatchOutcome Outcome => HomeSetsWon.CompareTo(AwaySetsWon) switch
    {
        > 0 => MatchOutcome.HomeWin,
        < 0 => MatchOutcome.AwayWin,
        _ => MatchOutcome.Undecided,
    };
}

public enum MatchOutcome
{
    Undecided = 0,
    HomeWin = 1,
    AwayWin = 2,
}
