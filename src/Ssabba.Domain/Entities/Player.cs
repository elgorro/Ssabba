namespace Ssabba.Domain.Entities;

/// <summary>A person who plays beach volleyball. Linked to an OIDC subject once they sign in.</summary>
public class Player
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string DisplayName { get; set; }

    /// <summary>The OIDC "sub" claim, or <c>null</c> for players entered manually by someone else.</summary>
    public string? SubjectId { get; set; }

    public int Rating { get; set; } = EloRating.InitialRating;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TeamMember> TeamMemberships { get; set; } = [];
}
