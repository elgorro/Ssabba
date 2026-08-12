namespace Ssabba.Domain.Entities;

/// <summary>
/// A way of playing, from 2v2 up to 6v6, with the scoring it conventionally uses. Reference data:
/// the rows are seeded and never authored by users, so a community varies them through a
/// <see cref="RuleSet"/> rather than by editing these.
/// </summary>
public class Format
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public FormatCode Code { get; set; }

    /// <summary>Players per side. Matches the numeric value of <see cref="Code"/>.</summary>
    public int PlayersPerSide { get; set; }

    public required string Name { get; set; }

    public int DefaultSetsToWin { get; set; }

    public int DefaultPointsPerSet { get; set; }

    /// <summary>Margin required to take a set, normally 2.</summary>
    public int DefaultWinBy { get; set; } = 2;

    /// <summary>Points in a deciding set, which is usually shorter.</summary>
    public int DefaultTiebreakPoints { get; set; }

    /// <summary>
    /// How heavily a result in this format moves a rating, in percent. Bigger sides dilute any one
    /// player's influence, so their results say less about them.
    /// </summary>
    public int RatingWeightPercent { get; set; } = 100;
}

/// <summary>The numeric value is the number of players per side, which several queries rely on.</summary>
public enum FormatCode
{
    TwoVsTwo = 2,
    ThreeVsThree = 3,
    FourVsFour = 4,
    FiveVsFive = 5,
    SixVsSix = 6,
}
