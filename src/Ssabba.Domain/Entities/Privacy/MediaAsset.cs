namespace Ssabba.Domain.Entities;

/// <summary>An uploaded file: a session photo, a sponsor logo, a receipt, a data export.</summary>
public class MediaAsset
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>Null for instance-level files such as a player's own data export.</summary>
    public Guid? CommunityId { get; set; }
    public Community? Community { get; set; }

    public Guid? UploadedByMemberId { get; set; }
    public CommunityMember? UploadedByMember { get; set; }

    public required string StoragePath { get; set; }

    public required string ContentType { get; set; }

    public long Bytes { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public ContactVisibility Visibility { get; set; } = ContactVisibility.Members;

    /// <summary>Content hash, so the same file is not stored twice.</summary>
    public string? Sha256 { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<MediaSubject> Subjects { get; set; } = [];
}
