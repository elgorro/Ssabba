namespace Ssabba.Domain;

/// <summary>
/// Whether a set of scores could have been played under a given set of rules. The check exists
/// because the commonest thing to happen at the net is a typo, and the commonest typo — a digit too
/// many, or two scores the wrong way round — produces a result that is quietly wrong rather than
/// obviously wrong, and then moves somebody's rating.
/// </summary>
/// <remarks>
/// Every rule here is read from the match's own snapshot rather than from any current rule set, so a
/// group that plays one set to 15 is never told it is wrong. Ssabba checks that a score is
/// <em>possible</em>, never that it is true: only the people who were there know that.
/// </remarks>
public static class MatchScoring
{
    /// <summary>The score of one set, as the validator sees it.</summary>
    public readonly record struct Set(int Number, int HomePoints, int AwayPoints);

    /// <summary>
    /// The first thing wrong with these scores, in words fit to show a person, or <c>null</c> when
    /// they are plausible.
    /// </summary>
    /// <param name="sets">The sets as entered, expected to number from 1 upwards.</param>
    /// <param name="setsToWin">Sets needed to take the match.</param>
    /// <param name="pointsPerSet">Points that win an ordinary set.</param>
    /// <param name="winBy">
    /// Clear points needed to take a set. One or zero turns the margin rule off, which is what a
    /// group playing straight to a number wants.
    /// </param>
    /// <param name="tiebreakPoints">
    /// Points that win the deciding set, when it is shorter. Zero means it is not.
    /// </param>
    public static string? Validate(
        IReadOnlyList<Set> sets,
        int setsToWin,
        int pointsPerSet,
        int winBy,
        int tiebreakPoints)
    {
        ArgumentNullException.ThrowIfNull(sets);
        ArgumentOutOfRangeException.ThrowIfLessThan(setsToWin, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pointsPerSet, 1);

        if (sets.Count == 0)
        {
            return "A match needs at least one set.";
        }

        var maxSets = (2 * setsToWin) - 1;

        if (sets.Count > maxSets)
        {
            return $"A match is at most {maxSets} sets, and {sets.Count} were entered.";
        }

        var ordered = sets.OrderBy(s => s.Number).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Number != i + 1)
            {
                return "Sets must be numbered from 1 upwards, without gaps or repeats.";
            }
        }

        var homeSetsWon = 0;
        var awaySetsWon = 0;

        foreach (var set in ordered)
        {
            // A set played after one side already had the match is not a set anybody played.
            if (homeSetsWon == setsToWin || awaySetsWon == setsToWin)
            {
                return $"The match was already won after {homeSetsWon + awaySetsWon} sets, "
                    + $"so set {set.Number} cannot have been played.";
            }

            var target = IsDecider(set.Number, setsToWin, tiebreakPoints) ? tiebreakPoints : pointsPerSet;

            if (Describe(set, target, winBy) is { } complaint)
            {
                return complaint;
            }

            if (set.HomePoints > set.AwayPoints)
            {
                homeSetsWon++;
            }
            else
            {
                awaySetsWon++;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether the match is finished: one side has the sets it needed. An unfinished match is a
    /// real thing — it is simply not one that can be confirmed or rated.
    /// </summary>
    public static bool IsDecided(IReadOnlyList<Set> sets, int setsToWin)
    {
        ArgumentNullException.ThrowIfNull(sets);

        return sets.Count(s => s.HomePoints > s.AwayPoints) >= setsToWin
            || sets.Count(s => s.AwayPoints > s.HomePoints) >= setsToWin;
    }

    /// <summary>The deciding set is the last one a match of this length can go to.</summary>
    private static bool IsDecider(int number, int setsToWin, int tiebreakPoints) =>
        tiebreakPoints > 0 && number == (2 * setsToWin) - 1;

    private static string? Describe(Set set, int target, int winBy)
    {
        if (set.HomePoints < 0 || set.AwayPoints < 0)
        {
            return $"Set {set.Number} has a negative score.";
        }

        if (set.HomePoints == set.AwayPoints)
        {
            return $"Set {set.Number} is level at {set.HomePoints}. A set is played until somebody wins it.";
        }

        var winner = Math.Max(set.HomePoints, set.AwayPoints);
        var loser = Math.Min(set.HomePoints, set.AwayPoints);

        if (winner < target)
        {
            return $"Set {set.Number} was won on {winner}, but a set here is played to {target}.";
        }

        var margin = winner - loser;

        if (margin < winBy)
        {
            return $"Set {set.Number} was won by {margin}, and {winBy} clear points are needed.";
        }

        // Past the target the set ends the moment the lead is clear, so 25-21 to 21 never happened:
        // it was over at 23-21. Only meaningful when a margin is required at all.
        if (winBy > 1 && winner > target && margin != winBy)
        {
            return $"Set {set.Number} went past {target}, so it must have been won by exactly "
                + $"{winBy}, not {margin}.";
        }

        return null;
    }
}
