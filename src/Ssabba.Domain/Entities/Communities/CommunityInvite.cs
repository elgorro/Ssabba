namespace Ssabba.Domain.Entities;

/// <summary>An organiser-initiated invitation to join a community.</summary>
public class CommunityInvite
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    /// <summary>Opaque single-use token carried in the invite link. Stored hashed.</summary>
    public required string TokenHash { get; set; }

    /// <summary>Optional: an invite addressed to someone in particular rather than a shareable link.</summary>
    public string? Email { get; set; }

    /// <summary>The role granted on acceptance.</summary>
    public CommunityRole Role { get; set; } = CommunityRole.Member;

    public Guid InvitedByMemberId { get; set; }
    public CommunityMember? InvitedByMember { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public Guid? AcceptedByPlayerId { get; set; }
    public Player? AcceptedByPlayer { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
