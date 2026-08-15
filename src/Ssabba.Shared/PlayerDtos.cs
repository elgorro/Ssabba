namespace Ssabba.Shared;

/// <summary>
/// A player as shown in the roster of one community. Flattened — including the enums — so the WASM
/// client needs no domain reference.
/// </summary>
public record PlayerSummary(
    Guid Id,
    string DisplayName,
    string Slug,
    /// <summary>What this player is called in this community, when it differs from the display name.</summary>
    string? Nickname,
    /// <summary>Role within the community: Guest, Member, Organizer, Admin or Owner.</summary>
    string Role,
    /// <summary>The player has left this community. Their history and rating are untouched.</summary>
    bool IsRetired,
    int Rating,
    int MatchesPlayed);

/// <summary>Everything the detail and edit pages show: identity, membership and the optional profile.</summary>
public record PlayerDetail(
    Guid Id,
    string DisplayName,
    string Slug,
    string? PreferredTimeZone,
    string? Locale,
    /// <summary><c>true</c> once the player has signed in and been linked to an OIDC subject.</summary>
    bool HasAccount,
    string? Nickname,
    string Role,
    bool IsRetired,
    int Rating,
    int MatchesPlayed,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LeftAt,
    PlayerProfileDto Profile);

/// <summary>Self-reported detail. Advisory throughout; none of it feeds the ladder.</summary>
public record PlayerProfileDto(
    int? HeightCm,
    /// <summary>Names of the preferred positions, e.g. ["Defender", "Blocker"].</summary>
    IReadOnlyList<string> PreferredPositions,
    /// <summary>Self-assessed level, 1 (beginner) to 10.</summary>
    int? SelfRatedLevel,
    int? PlayingSince,
    string? Bio,
    bool IsLeftHanded)
{
    public static PlayerProfileDto Empty { get; } = new(null, [], null, null, null, false);
}

/// <summary>Payload for adding a player to the community. Creates the identity and the membership.</summary>
public record CreatePlayerRequest(
    string DisplayName,
    /// <summary>Left empty, it is derived from the display name.</summary>
    string? Slug,
    string? PreferredTimeZone,
    string? Locale,
    string? Nickname,
    /// <summary>Defaults to Member when omitted.</summary>
    string? Role,
    PlayerProfileDto? Profile);

/// <summary>Payload for editing a player. Retiring is a separate call, not a field here.</summary>
public record UpdatePlayerRequest(
    string DisplayName,
    string? Slug,
    string? PreferredTimeZone,
    string? Locale,
    string? Nickname,
    string? Role,
    PlayerProfileDto? Profile);
