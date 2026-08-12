namespace Ssabba.Domain.Entities;

/// <summary>What members are asked to pay, and how often.</summary>
public class DuesPlan
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public Guid? SeasonId { get; set; }
    public Season? Season { get; set; }

    public required string Name { get; set; }

    public long AmountMinor { get; set; }

    public required string Currency { get; set; }

    public DuesPeriod Period { get; set; } = DuesPeriod.Season;

    /// <summary>Charge only members at this level, e.g. full members but not guests.</summary>
    public CommunityRole? AppliesToRole { get; set; }

    public bool IsActive { get; set; } = true;
}

public enum DuesPeriod
{
    Season = 0,
    Monthly = 1,
    Annual = 2,
    PerSession = 3,
    OneOff = 4,
}
