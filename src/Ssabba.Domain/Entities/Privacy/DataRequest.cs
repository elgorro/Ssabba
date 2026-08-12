namespace Ssabba.Domain.Entities;

/// <summary>A request from a player to receive or erase their data.</summary>
public class DataRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public DataRequestKind Kind { get; set; }

    public DataRequestStatus Status { get; set; } = DataRequestStatus.Requested;

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>The generated export, for a request that produced a file.</summary>
    public Guid? ResultMediaId { get; set; }
    public MediaAsset? ResultMedia { get; set; }

    public string? Note { get; set; }
}

public enum DataRequestKind
{
    Export = 0,
    Erasure = 1,
}

public enum DataRequestStatus
{
    Requested = 0,
    InProgress = 1,
    Completed = 2,
    Rejected = 3,
}
