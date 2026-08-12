namespace Ssabba.Domain.Entities;

/// <summary>A period a community's ladders, statistics and dues are scoped to.</summary>
public class Season
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public required string Name { get; set; }

    public DateOnly StartsOn { get; set; }

    public DateOnly? EndsOn { get; set; }

    /// <summary>At most one season per community carries this.</summary>
    public bool IsCurrent { get; set; }
}
