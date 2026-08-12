namespace Ssabba.Domain.Entities;

/// <summary>One thing that can be voted for.</summary>
public class PollOption
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid PollId { get; set; }
    public Poll? Poll { get; set; }

    public required string Label { get; set; }

    /// <summary>Set on date polls: the slot this option proposes.</summary>
    public DateTimeOffset? StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public Guid? CourtId { get; set; }
    public Court? Court { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Vote> Votes { get; set; } = [];
}
