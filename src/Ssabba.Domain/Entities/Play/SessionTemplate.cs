namespace Ssabba.Domain.Entities;

/// <summary>
/// A recurring fixture, e.g. "every Tuesday at 18:00". A background job materialises upcoming
/// <see cref="Session"/> rows from it, so a weekly round is one row rather than fifty-two.
/// </summary>
public class SessionTemplate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public required string Title { get; set; }

    /// <summary>RFC 5545 recurrence rule, e.g. "FREQ=WEEKLY;BYDAY=TU".</summary>
    public required string Rrule { get; set; }

    /// <summary>Wall-clock start in the community's time zone, so the hour survives a DST change.</summary>
    public TimeOnly StartTimeLocal { get; set; }

    public int DurationMinutes { get; set; } = 120;

    public Guid? CourtId { get; set; }
    public Court? Court { get; set; }

    public int? Capacity { get; set; }

    public Guid? DefaultRuleSetId { get; set; }
    public RuleSet? DefaultRuleSet { get; set; }

    /// <summary>How far ahead sessions are generated.</summary>
    public int GenerateAheadDays { get; set; } = 60;

    public bool IsActive { get; set; } = true;
}
