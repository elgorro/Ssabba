namespace Ssabba.Domain.Entities;

/// <summary>A physical site with one or more courts. May be public, or owned by a community.</summary>
public class Venue
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Name { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    /// <summary>The community that runs this site, or <c>null</c> for a public beach anyone may use.</summary>
    public Guid? OwnerCommunityId { get; set; }
    public Community? OwnerCommunity { get; set; }

    public VenueAccess Access { get; set; } = VenueAccess.Public;

    public string? Notes { get; set; }

    /// <summary>Opening hours as JSON, keyed by weekday. Free-form because every site differs.</summary>
    public string? OpeningHours { get; set; }

    public ICollection<Court> Courts { get; set; } = [];
}

public enum VenueAccess
{
    Public = 0,

    /// <summary>Open to members of the owning community.</summary>
    Membership = 1,

    Private = 2,
}
