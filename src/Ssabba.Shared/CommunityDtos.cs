namespace Ssabba.Shared;

/// <summary>
/// The community this instance is for. Flattened — including the enum — so the WASM client needs no
/// domain reference.
/// </summary>
public record CommunityDetail(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    /// <summary>IANA zone, e.g. "Europe/Berlin". Sessions are scheduled against this.</summary>
    string TimeZone,
    /// <summary>ISO-4217 code for every amount the community books.</summary>
    string Currency,
    /// <summary>Discoverability: Private, Unlisted or Public.</summary>
    string Visibility,
    /// <summary>Stable public identifier, handed to other instances when communities link up.</summary>
    Guid PublicKeyId,
    /// <summary>
    /// Minutes the people who played may go on correcting a result. Null takes the default of 60
    /// hours; zero leaves amending to organisers alone.
    /// </summary>
    int? AmendWindowMinutes,
    DateTimeOffset CreatedAt);

/// <summary>
/// Payload for first run: names the community and makes the caller its owner. Accepted only while
/// the instance has none.
/// </summary>
public record CreateCommunityRequest(
    string Name,
    /// <summary>Left empty, it is derived from the name.</summary>
    string? Slug,
    string? Description,
    string? TimeZone,
    string? Currency,
    string? Visibility,
    /// <summary>
    /// Minutes the people who played may go on correcting a result. Null takes the default of 60
    /// hours; zero leaves amending to organisers alone.
    /// </summary>
    int? AmendWindowMinutes = null);

/// <summary>
/// Payload for editing the community. <c>PublicKeyId</c> is absent by design: it survives a rename,
/// which is the whole point of it.
/// </summary>
public record UpdateCommunityRequest(
    string Name,
    string? Slug,
    string? Description,
    string? TimeZone,
    string? Currency,
    string? Visibility,
    /// <summary>
    /// Minutes the people who played may go on correcting a result. Null takes the default of 60
    /// hours; zero leaves amending to organisers alone.
    /// </summary>
    int? AmendWindowMinutes = null);
