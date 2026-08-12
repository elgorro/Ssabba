namespace Ssabba.Domain.Entities;

/// <summary>An optional grouping of matches, e.g. a weekend cup.</summary>
public class Tournament
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public required string Name { get; set; }

    public Guid FormatId { get; set; }
    public Format? Format { get; set; }

    public Guid? SeasonId { get; set; }
    public Season? Season { get; set; }

    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }

    public Guid? RuleSetId { get; set; }
    public RuleSet? RuleSet { get; set; }

    public TournamentType Type { get; set; } = TournamentType.RoundRobin;

    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;

    public DateOnly StartsOn { get; set; }

    public DateOnly? EndsOn { get; set; }

    public ICollection<Match> Matches { get; set; } = [];

    public ICollection<TournamentEntry> Entries { get; set; } = [];
}

public enum TournamentType
{
    /// <summary>Everyone plays everyone.</summary>
    RoundRobin = 0,

    SingleElimination = 1,
    DoubleElimination = 2,

    /// <summary>Paired by standing each round, without elimination.</summary>
    Swiss = 3,

    /// <summary>Winners hold the court; losers rotate off.</summary>
    KingOfTheCourt = 4,
}

public enum TournamentStatus
{
    Draft = 0,
    RegistrationOpen = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
}
