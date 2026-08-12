namespace Ssabba.Domain.Entities;

/// <summary>
/// One member's relationship to a session: what they said they would do, and what they did. Keeping
/// both is the point, because the gap between them is what no-show tracking is.
/// </summary>
public class SessionParticipant
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid SessionId { get; set; }
    public Session? Session { get; set; }

    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public ParticipationResponse Response { get; set; } = ParticipationResponse.Maybe;

    public DateTimeOffset RespondedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Position in the queue when the session is full; <c>null</c> when they hold a place.</summary>
    public int? WaitlistPosition { get; set; }

    public AttendanceState Attendance { get; set; } = AttendanceState.Unknown;

    /// <summary>
    /// Set when this row is a plus-one: the member who brought them, and who answers for them.
    /// Lets someone bring a friend who has no account.
    /// </summary>
    public Guid? IsGuestOfMemberId { get; set; }
    public CommunityMember? IsGuestOfMember { get; set; }

    public string? Note { get; set; }
}

public enum ParticipationResponse
{
    No = 0,
    Maybe = 1,
    Yes = 2,

    /// <summary>Said yes, but the session was already full.</summary>
    Waitlisted = 3,
}

public enum AttendanceState
{
    /// <summary>Not yet recorded.</summary>
    Unknown = 0,

    Present = 1,

    /// <summary>Committed and did not appear. Counts against reliability.</summary>
    NoShow = 2,

    /// <summary>Cancelled in good time; does not count against reliability.</summary>
    Excused = 3,
}
