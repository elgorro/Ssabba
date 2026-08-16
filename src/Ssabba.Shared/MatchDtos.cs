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

/// <summary>A single match with everything needed to show or edit it.</summary>
public record MatchDetail(
    Guid Id,
    DateTimeOffset PlayedAt,
    /// <summary>Court name where known, otherwise the free-text note. Ready to render.</summary>
    string? Location,
    string? LocationNote,
    Guid HomeTeamId,
    string HomeTeam,
    Guid AwayTeamId,
    string AwayTeam,
    IReadOnlyList<SetScore> Sets,
    int HomeSetsWon,
    int AwaySetsWon,
    /// <summary>Derived from the sets, never entered: "HomeWin", "AwayWin" or "Undecided".</summary>
    string Outcome,
    string Status,
    /// <summary>
    /// The scoring the match was played under, copied down when it was recorded. It travels so the
    /// entry form can check a score against the same rules the server will.
    /// </summary>
    int SetsToWin,
    int PointsPerSet,
    int WinBy,
    int TiebreakPoints);

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

/// <summary>Payload for correcting a recorded match. Same shape as creating one.</summary>
public record UpdateMatchRequest(
    DateTimeOffset PlayedAt,
    Guid? CourtId,
    string? LocationNote,
    Guid HomeTeamId,
    Guid AwayTeamId,
    IReadOnlyList<SetScore> Sets);
