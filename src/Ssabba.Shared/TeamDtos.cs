namespace Ssabba.Shared;

/// <summary>A team as shown in lists and in the pickers. Flattened for the WASM client.</summary>
public record TeamSummary(
    Guid Id,
    /// <summary>The name, or the members joined with slashes when the team has none. Ready to render.</summary>
    string DisplayName,
    string? Name,
    /// <summary>Thrown together for one match, rather than a pairing that plays on.</summary>
    bool IsAdHoc,
    IReadOnlyList<string> MemberNames,
    int MatchesPlayed);

/// <summary>Everything the team page shows: who plays in it and how it has done.</summary>
public record TeamDetail(
    Guid Id,
    string DisplayName,
    string? Name,
    bool IsAdHoc,
    IReadOnlyList<TeamMemberDto> Members,
    int Wins,
    int Losses,
    int MatchesPlayed);

public record TeamMemberDto(
    Guid PlayerId,
    string DisplayName,
    string Slug,
    /// <summary>Name of the position played, e.g. "Defender", or "None" when unrecorded.</summary>
    string Position,
    int SortOrder);

/// <summary>One row of a team's match list, already told from that team's side of the net.</summary>
public record TeamMatchRow(
    Guid MatchId,
    DateTimeOffset PlayedAt,
    string Opponent,
    bool Won,
    int SetsFor,
    int SetsAgainst);

/// <summary>
/// Payload for forming a team. A roster already on record is returned rather than duplicated, so
/// this is a request to have the team exist, not necessarily to create one.
/// </summary>
public record CreateTeamRequest(
    string? Name,
    bool IsAdHoc,
    IReadOnlyList<Guid> PlayerIds);

/// <summary>Payload for editing a team. Changing the members re-keys it.</summary>
public record UpdateTeamRequest(
    string? Name,
    bool IsAdHoc,
    IReadOnlyList<Guid> PlayerIds);
