namespace Ssabba.Domain.Entities;

/// <summary>A piece of kit the community owns: balls, nets, lines, the pump nobody can ever find.</summary>
public class EquipmentItem
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public required string Name { get; set; }

    public EquipmentKind Kind { get; set; } = EquipmentKind.Other;

    /// <summary>Whatever is written on the sticker, when the community labels its kit.</summary>
    public string? AssetTag { get; set; }

    public DateOnly? PurchasedOn { get; set; }

    public long? PurchasePriceMinor { get; set; }

    public string? Currency { get; set; }

    public EquipmentCondition Condition { get; set; } = EquipmentCondition.Good;

    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;

    /// <summary>Where it normally lives.</summary>
    public Guid? HomeVenueId { get; set; }
    public Venue? HomeVenue { get; set; }

    public string? Notes { get; set; }
}

public enum EquipmentKind
{
    Ball = 0,
    Net = 1,
    Antenna = 2,
    Line = 3,
    Pump = 4,
    Rake = 5,
    Whistle = 6,
    Scoreboard = 7,
    Bag = 8,
    Other = 9,
}

public enum EquipmentCondition
{
    New = 0,
    Good = 1,
    Worn = 2,
    Damaged = 3,
    Retired = 4,
}

public enum EquipmentStatus
{
    Available = 0,
    CheckedOut = 1,
    InRepair = 2,
    Lost = 3,
    Retired = 4,
}
