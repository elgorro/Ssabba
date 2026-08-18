namespace Ssabba.Domain.Entities;

/// <summary>
/// A group of people who play together: a club, a regular round, a beach. Everything a group owns
/// hangs off a community. An instance normally runs exactly one; the schema permits several, but
/// there is no tenancy framework behind it — see ADR-0002.
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

    /// <summary>
    /// How long a participant may go on correcting a result they played in, in minutes from when it
    /// was entered. A single match may override it. <c>null</c> takes the app's default of 60 hours;
    /// <c>0</c> leaves amending to organisers alone.
    /// </summary>
    public int? AmendWindowMinutes { get; set; }

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
