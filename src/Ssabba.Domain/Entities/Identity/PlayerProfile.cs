namespace Ssabba.Domain.Entities;

/// <summary>Optional self-reported detail about a player. One row per player at most.</summary>
public class PlayerProfile
{
    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public int? HeightCm { get; set; }

    public PlayingPosition PreferredPositions { get; set; } = PlayingPosition.None;

    /// <summary>Self-assessed level, 1 (beginner) to 10. Advisory only; it never feeds the ladder.</summary>
    public int? SelfRatedLevel { get; set; }

    public int? PlayingSince { get; set; }

    public string? Bio { get; set; }

    public bool IsLeftHanded { get; set; }
}

/// <summary>Positions a player is happy in. Combinable, since most people cover several.</summary>
[Flags]
public enum PlayingPosition
{
    None = 0,
    Defender = 1 << 0,
    Blocker = 1 << 1,
    Setter = 1 << 2,
    Outside = 1 << 3,
    Opposite = 1 << 4,
    Middle = 1 << 5,
    Libero = 1 << 6,
}
