namespace Ssabba.Domain.Entities;

/// <summary>
/// A record that someone agreed to something, or later withdrew it. Append-only: withdrawing
/// consent writes a new row rather than amending the old one, so what was true at the time a photo
/// was published stays answerable.
/// </summary>
public class ConsentRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    /// <summary>Null when the consent applies across the whole instance rather than one community.</summary>
    public Guid? CommunityId { get; set; }
    public Community? Community { get; set; }

    public ConsentKind Kind { get; set; }

    public bool Granted { get; set; }

    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Which version of the policy was agreed to.</summary>
    public string? PolicyVersion { get; set; }

    /// <summary>How it was collected, e.g. "signup form" or "verbally, recorded by an admin".</summary>
    public string? Source { get; set; }
}

public enum ConsentKind
{
    /// <summary>May appear in photos the community publishes.</summary>
    Photos = 0,

    /// <summary>Contact details may be shown to fellow members.</summary>
    ContactSharing = 1,

    Newsletter = 2,

    /// <summary>Rating and ranking may be shown outside the community.</summary>
    RatingPublic = 3,

    Analytics = 4,
}
