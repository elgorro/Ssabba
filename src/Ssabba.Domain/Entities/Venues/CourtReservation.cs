namespace Ssabba.Domain.Entities;

/// <summary>
/// A block of time held on a court: a booking the community made, or a period it is unavailable.
/// Distinct from a session, which is the gathering that may occupy the reservation.
/// </summary>
public class CourtReservation
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CourtId { get; set; }
    public Court? Court { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public Guid? HeldByCommunityId { get; set; }
    public Community? HeldByCommunity { get; set; }

    public Guid? HeldByMemberId { get; set; }
    public CommunityMember? HeldByMember { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    /// <summary>What the slot cost, in minor units of <see cref="Currency"/>.</summary>
    public long? CostMinor { get; set; }

    public string? Currency { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum ReservationStatus
{
    /// <summary>Holds the slot. Only these are checked for overlap.</summary>
    Active = 0,

    Cancelled = 1,

    /// <summary>Requested but not yet granted by the venue.</summary>
    Tentative = 2,
}
