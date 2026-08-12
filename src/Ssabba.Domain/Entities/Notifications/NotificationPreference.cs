namespace Ssabba.Domain.Entities;

/// <summary>How, and whether, one member wants to hear about one kind of thing.</summary>
public class NotificationPreference
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public NotificationKind Kind { get; set; }

    public NotificationChannel Channels { get; set; } = NotificationChannel.Email;

    /// <summary>How far ahead to send a reminder, where the notification is a reminder.</summary>
    public int? LeadTimeMinutes { get; set; }
}

public enum NotificationKind
{
    SessionReminder = 0,
    SessionCancelled = 1,
    PollOpened = 2,
    PollClosing = 3,
    MatchConfirmation = 4,
    DuesDue = 5,
    ServiceRequestUpdate = 6,
    WaitlistPromoted = 7,
}

/// <summary>Combinable: the same notice may go out by more than one route.</summary>
[Flags]
public enum NotificationChannel
{
    None = 0,
    Email = 1 << 0,
    Push = 1 << 1,
    WebOnly = 1 << 2,
}
