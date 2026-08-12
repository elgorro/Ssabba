namespace Ssabba.Domain.Entities;

/// <summary>A team taking part in a tournament.</summary>
public class TournamentEntry
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    /// <summary>Seeding position, or <c>null</c> when the draw is unseeded.</summary>
    public int? Seed { get; set; }

    public EntryStatus Status { get; set; } = EntryStatus.Registered;

    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Final placing once the tournament is over.</summary>
    public int? FinalRank { get; set; }
}

public enum EntryStatus
{
    Registered = 0,
    Confirmed = 1,
    Withdrawn = 2,
    Disqualified = 3,
}
