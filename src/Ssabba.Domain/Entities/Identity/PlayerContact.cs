namespace Ssabba.Domain.Entities;

/// <summary>
/// One way to reach a player. Kept apart from <see cref="Player"/> so it can be exported, hidden or
/// erased as a unit, and so each channel carries its own visibility.
/// </summary>
public class PlayerContact
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public ContactKind Kind { get; set; }

    public required string Value { get; set; }

    public string? Label { get; set; }

    /// <summary>Defaults to admins only: contact details are never shared wider without a decision.</summary>
    public ContactVisibility Visibility { get; set; } = ContactVisibility.Admins;

    public DateTimeOffset? VerifiedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum ContactKind
{
    Email = 0,
    Phone = 1,
    WhatsApp = 2,
    Signal = 3,
    Instagram = 4,
    Telegram = 5,
    Website = 6,
    Other = 7,
}

public enum ContactVisibility
{
    /// <summary>Visible to the player alone.</summary>
    Private = 0,

    /// <summary>Visible to admins and organisers of communities the player belongs to.</summary>
    Admins = 1,

    /// <summary>Visible to fellow members.</summary>
    Members = 2,

    /// <summary>Visible to anyone, including signed-out visitors.</summary>
    Public = 3,
}
