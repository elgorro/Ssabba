namespace Ssabba.Domain.Entities;

/// <summary>Money from outside the membership: a sponsor, a grant, a donation, a fundraiser.</summary>
public class FundingSource
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public required string Name { get; set; }

    public FundingKind Kind { get; set; } = FundingKind.Sponsor;

    /// <summary>Who to talk to, when that is a person the community already knows.</summary>
    public Guid? ContactPlayerId { get; set; }
    public Player? ContactPlayer { get; set; }

    public string? ContactDetails { get; set; }

    /// <summary>What was promised. What actually arrived is in the ledger.</summary>
    public long? AmountMinor { get; set; }

    public string? Currency { get; set; }

    public DateOnly? StartsOn { get; set; }

    public DateOnly? EndsOn { get; set; }

    public FundingStatus Status { get; set; } = FundingStatus.Prospective;

    public Guid? LogoMediaId { get; set; }
    public MediaAsset? LogoMedia { get; set; }

    public string? Notes { get; set; }
}

public enum FundingKind
{
    Sponsor = 0,
    Grant = 1,
    Donation = 2,
    Crowdfunding = 3,
}

public enum FundingStatus
{
    /// <summary>Being approached.</summary>
    Prospective = 0,

    Committed = 1,
    Active = 2,
    Completed = 3,
    Declined = 4,
}
