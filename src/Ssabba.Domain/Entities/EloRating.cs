namespace Ssabba.Domain.Entities;

/// <summary>Elo rating maths used for the player ladder.</summary>
public static class EloRating
{
    public const int InitialRating = 1000;

    /// <summary>K-factor: how much a single result can move a rating.</summary>
    public const int KFactor = 24;

    /// <summary>Probability that a player rated <paramref name="rating"/> beats <paramref name="opponentRating"/>.</summary>
    public static double ExpectedScore(int rating, int opponentRating) =>
        1.0 / (1.0 + Math.Pow(10, (opponentRating - rating) / 400.0));

    /// <summary>
    /// New rating after a result. <paramref name="actualScore"/> is 1 for a win, 0.5 for a draw, 0 for a loss.
    /// </summary>
    public static int Apply(int rating, int opponentRating, double actualScore)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(actualScore, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(actualScore, 1);

        return (int)Math.Round(rating + KFactor * (actualScore - ExpectedScore(rating, opponentRating)));
    }
}
