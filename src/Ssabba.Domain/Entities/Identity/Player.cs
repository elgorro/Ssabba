namespace Ssabba.Domain.Entities;

/// <summary>
/// A person who plays beach volleyball, identified once across the whole instance. Linked to an OIDC
/// subject after they sign in; a player with no subject is someone entered by hand.
/// </summary>
/// <remarks>
/// A player carries no rating. Standing is relative to the people you play with, so it lives on
/// <see cref="CommunityMember"/> instead.
/// </remarks>
public class Player
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string DisplayName { get; set; }

    /// <summary>URL-safe identifier, unique across the instance.</summary>
    public required string Slug { get; set; }

    /// <summary>The OIDC "sub" claim, or <c>null</c> for players entered manually by someone else.</summary>
    public string? SubjectId { get; set; }

    /// <summary>IANA zone used to render times for this player; falls back to the community's.</summary>
    public string? PreferredTimeZone { get; set; }

    /// <summary>BCP-47 tag, e.g. "de-CH".</summary>
    public string? Locale { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Set when the player is erased; rows are kept so past matches stay readable.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public PlayerProfile? Profile { get; set; }

    public ICollection<PlayerContact> Contacts { get; set; } = [];

    public ICollection<CommunityMember> Memberships { get; set; } = [];

    public ICollection<TeamMember> TeamMemberships { get; set; } = [];
}
