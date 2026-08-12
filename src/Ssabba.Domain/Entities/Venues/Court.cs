namespace Ssabba.Domain.Entities;

/// <summary>A single playing surface at a venue.</summary>
public class Court
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid VenueId { get; set; }
    public Venue? Venue { get; set; }

    /// <summary>Court name or number as it is known on site, e.g. "3" or "Nordfeld".</summary>
    public required string Name { get; set; }

    public CourtSurface Surface { get; set; } = CourtSurface.Sand;

    public int? NetHeightCm { get; set; }

    public bool HasLighting { get; set; }

    /// <summary>Largest side this court comfortably takes; a beach court is usually 2.</summary>
    public int MaxTeamSize { get; set; } = 2;

    public bool IsActive { get; set; } = true;
}

public enum CourtSurface
{
    Sand = 0,
    Grass = 1,
    Indoor = 2,
    Hard = 3,
}
