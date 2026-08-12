namespace Ssabba.Domain.Entities;

/// <summary>A single game between two teams, made up of one or more sets.</summary>
public class Match
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public DateTimeOffset PlayedAt { get; set; }

    /// <summary>The gathering this was played at, when it was part of one.</summary>
    public Guid? SessionId { get; set; }
    public Session? Session { get; set; }

    public Guid FormatId { get; set; }
    public Format? Format { get; set; }

    public Guid? SeasonId { get; set; }
    public Season? Season { get; set; }

    /// <summary>Where it was played, when the court is known.</summary>
    public Guid? CourtId { get; set; }
    public Court? Court { get; set; }

    /// <summary>Free-text location, for matches played somewhere not on record as a court.</summary>
    public string? LocationNote { get; set; }

    public Guid HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }

    public Guid AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }

    public Guid? TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    /// <summary>Round within the tournament, counting from 1.</summary>
    public int? TournamentRound { get; set; }

    /// <summary>Position within the round's bracket, for drawing the tree.</summary>
    public int? BracketSlot { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Draft;

    /// <summary>Who entered the result. Null for matches recorded before this was tracked.</summary>
    public Guid? RecordedByMemberId { get; set; }
    public CommunityMember? RecordedByMember { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    /// <summary>
    /// When this match's result was folded into player ratings. Its presence makes applying ratings
    /// idempotent: a replay or a retry cannot count the same match twice.
    /// </summary>
    public DateTimeOffset? RatingAppliedAt { get; set; }

    public Guid? RuleSetId { get; set; }
    public RuleSet? RuleSet { get; set; }

    /// <summary>
    /// The scoring actually played under, copied from the rule set at the time. Amending a rule set
    /// must not silently rewrite what happened.
    /// </summary>
    public int SetsToWin { get; set; }

    public int PointsPerSet { get; set; }

    public int WinBy { get; set; } = 2;

    public int TiebreakPoints { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<MatchSet> Sets { get; set; } = [];

    /// <summary>Who actually played, and what it did to their ratings.</summary>
    public ICollection<MatchAppearance> Appearances { get; set; } = [];

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

public enum MatchStatus
{
    /// <summary>Being entered; incomplete.</summary>
    Draft = 0,

    /// <summary>Entered by one side and waiting to be agreed. Does not affect ratings.</summary>
    AwaitingConfirmation = 1,

    /// <summary>Agreed. Only these count towards ratings and statistics.</summary>
    Confirmed = 2,

    /// <summary>Challenged; ratings are held or rolled back until it is settled.</summary>
    Disputed = 3,

    /// <summary>Struck from the record, e.g. entered twice.</summary>
    Voided = 4,
}
