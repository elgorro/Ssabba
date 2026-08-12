namespace Ssabba.Domain.Entities;

/// <summary>
/// A notification queued for delivery. Written in the same transaction as the change that caused it,
/// so a notice is never sent for something that was rolled back, nor lost because sending failed.
/// A background service drains this.
/// </summary>
public class NotificationOutbox
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid? CommunityId { get; set; }
    public Community? Community { get; set; }

    public NotificationKind Kind { get; set; }

    public Guid RecipientPlayerId { get; set; }
    public Player? RecipientPlayer { get; set; }

    public NotificationChannel Channel { get; set; }

    /// <summary>Everything the renderer needs, as JSON, so sending does not re-read the world.</summary>
    public string? Payload { get; set; }

    public DateTimeOffset ScheduledFor { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? SentAt { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }

    /// <summary>Set once it has failed too often to keep trying.</summary>
    public DateTimeOffset? AbandonedAt { get; set; }
}
