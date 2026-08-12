namespace Ssabba.Domain.Entities;

/// <summary>An optional grouping of matches, e.g. a weekend cup.</summary>
public class Tournament
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Name { get; set; }

    public DateOnly StartsOn { get; set; }

    public DateOnly? EndsOn { get; set; }

    public ICollection<Match> Matches { get; set; } = [];
}
