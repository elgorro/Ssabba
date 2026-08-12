namespace Ssabba.Shared;

/// <summary>A match as shown in lists. Flattened so the WASM client needs no domain reference.</summary>
public record MatchSummary(
    Guid Id,
    DateTimeOffset PlayedAt,
    /// <summary>Court name where known, otherwise the free-text note. Ready to render.</summary>
    string? Location,
    string HomeTeam,
    string AwayTeam,
    int HomeSetsWon,
    int AwaySetsWon);

/// <summary>Payload for creating a match together with its set scores.</summary>
public record CreateMatchRequest(
    DateTimeOffset PlayedAt,
    Guid? CourtId,
    /// <summary>Used when the match was not played on a court on record.</summary>
    string? LocationNote,
    Guid HomeTeamId,
    Guid AwayTeamId,
    IReadOnlyList<SetScore> Sets);

public record SetScore(int Number, int HomePoints, int AwayPoints);

public record TeamOption(Guid Id, string Name);
