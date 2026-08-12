namespace Ssabba.Domain.Entities;

/// <summary>
/// A group of people who play together: a club, a regular round, a beach. Everything a group owns
/// hangs off a community, and a self-hosted instance may run more than one.
/// </summary>
public class Community
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Name { get; set; }

    /// <summary>URL-safe identifier, unique across the instance.</summary>
    public required string Slug { get; set; }

    public string? Description { get; set; }

    /// <summary>IANA zone, e.g. "Europe/Berlin". Sessions are scheduled against this.</summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>ISO-4217 code for every amount the community books.</summary>
    public string Currency { get; set; } = "EUR";

    public CommunityVisibility Visibility { get; set; } = CommunityVisibility.Private;

    /// <summary>
    /// Stable public identifier, handed to other instances when communities link up. Assigned once
    /// and never reused, so links survive a rename.
    /// </summary>
    public Guid PublicKeyId { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<CommunityMember> Members { get; set; } = [];

    public ICollection<Season> Seasons { get; set; } = [];
}

public enum CommunityVisibility
{
    /// <summary>Invite only; not discoverable.</summary>
    Private = 0,

    /// <summary>Reachable by direct link, but not listed.</summary>
    Unlisted = 1,

    /// <summary>Listed publicly and open to join requests.</summary>
    Public = 2,
}
