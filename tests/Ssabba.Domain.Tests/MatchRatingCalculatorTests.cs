using Ssabba.Domain.Entities;
using Ssabba.Domain.Rating;

namespace Ssabba.Domain.Tests;

public class MatchRatingCalculatorTests
{
    private const int Start = EloRating.InitialRating;

    [Fact]
    public void EvenlyMatchedWinnerGainsWhatLoserLoses()
    {
        var result = MatchRatingCalculator.Compute([Start, Start], [Start, Start], homeSetsWon: 2, awaySetsWon: 0);

        Assert.Equal(-Total(result.Away), Total(result.Home));
        Assert.True(Total(result.Home) > 0);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(2, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    public void RatingIsAlwaysZeroSum(int homeSets, int awaySets)
    {
        // Uneven sides and untidy ratings are where a naive split leaks points.
        var result = MatchRatingCalculator.Compute([1180, 975, 1042], [1310, 1000], homeSets, awaySets);

        Assert.Equal(0, Total(result.Home) + Total(result.Away));
    }

    [Fact]
    public void DrawnMatchStillMovesUnevenRatings()
    {
        var result = MatchRatingCalculator.Compute([1400], [1000], homeSetsWon: 1, awaySetsWon: 1);

        // The favourite drew, so it cost them.
        Assert.True(Total(result.Home) < 0);
        Assert.Equal(0, Total(result.Home) + Total(result.Away));
    }

    [Fact]
    public void BeatingAStrongerSideGainsMoreThanBeatingAWeakerOne()
    {
        var vsStronger = MatchRatingCalculator.Compute([1000], [1400], 2, 0);
        var vsWeaker = MatchRatingCalculator.Compute([1000], [600], 2, 0);

        Assert.True(Total(vsStronger.Home) > Total(vsWeaker.Home));
    }

    [Fact]
    public void ClearWinMovesRatingsFurtherThanNarrowOne()
    {
        var straightSets = MatchRatingCalculator.Compute([Start], [Start], 3, 0);
        var narrow = MatchRatingCalculator.Compute([Start], [Start], 3, 2);

        Assert.True(Total(straightSets.Home) > Total(narrow.Home));
    }

    [Fact]
    public void LargerFormatsMoveRatingsLess()
    {
        // A 6v6 result says less about any one player than a 2v2 result does.
        var twos = MatchRatingCalculator.Compute([Start, Start], [Start, Start], 2, 0, formatWeightPercent: 100);
        var sixes = MatchRatingCalculator.Compute([Start, Start], [Start, Start], 2, 0, formatWeightPercent: 50);

        Assert.True(Total(twos.Home) > Total(sixes.Home));
    }

    [Fact]
    public void SideSharesItsChangeEvenlyAndLosesNothingToRounding()
    {
        var result = MatchRatingCalculator.Compute([Start, Start, Start], [1500, 1500, 1500], 2, 0);

        var deltas = result.Home.Select(d => d.Delta).ToList();
        Assert.All(deltas, d => Assert.InRange(d, deltas.Min(), deltas.Min() + 1));
        Assert.Equal(Total(result.Home), deltas.Sum());
    }

    [Fact]
    public void AfterIsBeforePlusDelta()
    {
        var result = MatchRatingCalculator.Compute([1180, 975], [1042, 1310], 2, 1);

        Assert.All(result.Home.Concat(result.Away), d => Assert.Equal(d.Before + d.Delta, d.After));
    }

    [Fact]
    public void ComputingTwiceGivesTheSameAnswer()
    {
        // Replaying a community's history has to land on the number it landed on the first time.
        var first = MatchRatingCalculator.Compute([1180, 975], [1042, 1310], 2, 1);
        var second = MatchRatingCalculator.Compute([1180, 975], [1042, 1310], 2, 1);

        Assert.Equal(first.Home, second.Home);
        Assert.Equal(first.Away, second.Away);
    }

    [Fact]
    public void RejectsASideWithNobodyInIt()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MatchRatingCalculator.Compute([], [Start], 2, 0));
    }

    private static int Total(IReadOnlyList<MatchRatingCalculator.PlayerDelta> side) =>
        side.Sum(d => d.Delta);
}
