namespace Ssabba.Domain.Entities;

/// <summary>
/// A federation link to a community on another instance, so two groups can eventually share
/// tournaments or court bookings. Nothing consumes this yet; it exists so the identifiers stay
/// stable and the schema does not need rewriting when federation arrives. See ADR-0002 for the
/// reasoning and the questions still open.
/// </summary>
public class CommunityLink
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid SourceCommunityId { get; set; }
    public Community? SourceCommunity { get; set; }

    /// <summary>Base address of the remote instance's community, e.g. "https://beach.example/c/duenen".</summary>
    public required string TargetCommunityUri { get; set; }

    /// <summary>The remote community's <see cref="Community.PublicKeyId"/>, once the handshake confirms it.</summary>
    public Guid? TargetPublicKeyId { get; set; }

    public string? TargetName { get; set; }

    public CommunityLinkKind Kind { get; set; } = CommunityLinkKind.SharedTournaments;

    public CommunityLinkStatus Status { get; set; } = CommunityLinkStatus.Proposed;

    /// <summary>Hash of the secret both sides present when they talk. Never stored in the clear.</summary>
    public string? SharedSecretHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ConfirmedAt { get; set; }
}

public enum CommunityLinkKind
{
    SharedTournaments = 0,
    SharedCourts = 1,
    Full = 2,
}

public enum CommunityLinkStatus
{
    Proposed = 0,
    Active = 1,
    Suspended = 2,
    Revoked = 3,
}
