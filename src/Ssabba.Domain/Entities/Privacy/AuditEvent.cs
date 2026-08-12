namespace Ssabba.Domain.Entities;

/// <summary>
/// Who changed what, and when. Written for the things that cause arguments: scores, membership and
/// roles, and money.
/// </summary>
public class AuditEvent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid? CommunityId { get; set; }
    public Community? Community { get; set; }

    /// <summary>Null for actions taken by the system itself, e.g. a scheduled job.</summary>
    public Guid? ActorPlayerId { get; set; }
    public Player? ActorPlayer { get; set; }

    /// <summary>What happened, e.g. "match.score.changed".</summary>
    public required string Action { get; set; }

    public required string EntityType { get; set; }

    public Guid EntityId { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Before and after, as JSON. Shape varies by action, so it is not modelled further.</summary>
    public string? Data { get; set; }

    /// <summary>Hashed, never stored raw: enough to spot a pattern, not enough to track someone.</summary>
    public string? IpHash { get; set; }
}
