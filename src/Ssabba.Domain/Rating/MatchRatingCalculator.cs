using Ssabba.Domain.Entities;

namespace Ssabba.Domain.Rating;

/// <summary>
/// Turns a finished match into per-player rating changes. Sits on top of <see cref="EloRating"/>,
/// which knows only about two numbers; this knows about sides, formats and margins.
/// </summary>
/// <remarks>
/// Two properties are deliberate and tested. The result is <em>zero-sum</em>: whatever one side
/// gains, the other loses exactly, so the total rating in a community never drifts. And it is
/// <em>pure</em>: the same inputs always give the same output, which is what allows a community's
/// ratings to be rebuilt from its match history.
/// </remarks>
public static class MatchRatingCalculator
{
    /// <summary>What one player's rating did.</summary>
    /// <param name="Before">Rating going in.</param>
    /// <param name="After">Rating coming out.</param>
    /// <param name="Delta">The change, kept explicitly so history survives a rounding change.</param>
    public readonly record struct PlayerDelta(int Before, int After, int Delta);

    public readonly record struct Result(IReadOnlyList<PlayerDelta> Home, IReadOnlyList<PlayerDelta> Away);

    /// <summary>
    /// Rates a match. Each side is represented by the mean rating of the players who appeared, and
    /// the resulting change is shared equally among them.
    /// </summary>
    /// <param name="homeRatings">Ratings of the home players before the match. Must not be empty.</param>
    /// <param name="awayRatings">Ratings of the away players before the match. Must not be empty.</param>
    /// <param name="homeSetsWon">Sets won by the home side.</param>
    /// <param name="awaySetsWon">Sets won by the away side.</param>
    /// <param name="formatWeightPercent">
    /// <see cref="Format.RatingWeightPercent"/>: how much a result in this format is allowed to move
    /// a rating. Bigger sides dilute any one player's influence, so their results say less.
    /// </param>
    public static Result Compute(
        IReadOnlyList<int> homeRatings,
        IReadOnlyList<int> awayRatings,
        int homeSetsWon,
        int awaySetsWon,
        int formatWeightPercent = 100)
    {
        ArgumentOutOfRangeException.ThrowIfZero(homeRatings.Count);
        ArgumentOutOfRangeException.ThrowIfZero(awayRatings.Count);
        ArgumentOutOfRangeException.ThrowIfNegative(homeSetsWon);
        ArgumentOutOfRangeException.ThrowIfNegative(awaySetsWon);
        ArgumentOutOfRangeException.ThrowIfNegative(formatWeightPercent);

        var homeTeamRating = Mean(homeRatings);
        var awayTeamRating = Mean(awayRatings);

        var actualScore = homeSetsWon.CompareTo(awaySetsWon) switch
        {
            > 0 => 1.0,
            < 0 => 0.0,
            _ => 0.5,
        };

        var k = EloRating.KFactor
            * (formatWeightPercent / 100.0)
            * MarginMultiplier(homeSetsWon, awaySetsWon);

        var expected = EloRating.ExpectedScore(homeTeamRating, awayTeamRating);
        var homeTotal = (int)Math.Round(k * (actualScore - expected), MidpointRounding.AwayFromZero);

        // The away side receives the exact negation, which is what keeps the books balanced.
        return new Result(
            Apply(homeRatings, homeTotal),
            Apply(awayRatings, -homeTotal));
    }

    /// <summary>
    /// A clear win moves ratings further than a scrappy one. A 2-0 counts fully; every extra set the
    /// loser took damps the swing, and a draw is neutral.
    /// </summary>
    private static double MarginMultiplier(int homeSetsWon, int awaySetsWon)
    {
        var margin = Math.Abs(homeSetsWon - awaySetsWon);
        return margin <= 1 ? 1.0 : 1.0 + (0.25 * (margin - 1));
    }

    private static int Mean(IReadOnlyList<int> ratings)
    {
        var sum = 0L;
        foreach (var rating in ratings)
        {
            sum += rating;
        }

        return (int)Math.Round((double)sum / ratings.Count, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Splits a side's total change across its players. Integer division leaves a remainder, so the
    /// first few players absorb one point more; the parts always sum back to <paramref name="total"/>.
    /// </summary>
    private static PlayerDelta[] Apply(IReadOnlyList<int> ratings, int total)
    {
        var count = ratings.Count;
        var share = Math.Abs(total) / count;
        var remainder = Math.Abs(total) % count;
        var sign = Math.Sign(total);

        var deltas = new PlayerDelta[count];
        for (var i = 0; i < count; i++)
        {
            var delta = sign * (share + (i < remainder ? 1 : 0));
            deltas[i] = new PlayerDelta(ratings[i], ratings[i] + delta, delta);
        }

        return deltas;
    }
}
