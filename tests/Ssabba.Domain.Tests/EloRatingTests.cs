using Ssabba.Domain.Entities;

namespace Ssabba.Domain.Tests;

public class EloRatingTests
{
    [Fact]
    public void EqualRatings_ExpectHalfAChance()
    {
        Assert.Equal(0.5, EloRating.ExpectedScore(1000, 1000), precision: 6);
    }

    [Fact]
    public void WinningAgainstAnEqualOpponent_GainsHalfTheKFactor()
    {
        Assert.Equal(1000 + EloRating.KFactor / 2, EloRating.Apply(1000, 1000, actualScore: 1));
    }

    [Fact]
    public void BeatingAStrongerOpponent_GainsMoreThanBeatingAWeakerOne()
    {
        var vsStronger = EloRating.Apply(1000, 1400, actualScore: 1) - 1000;
        var vsWeaker = EloRating.Apply(1000, 600, actualScore: 1) - 1000;

        Assert.True(vsStronger > vsWeaker);
    }

    [Fact]
    public void RatingChangesAreZeroSumForAPair()
    {
        var winnerGain = EloRating.Apply(1200, 900, actualScore: 1) - 1200;
        var loserLoss = 900 - EloRating.Apply(900, 1200, actualScore: 0);

        Assert.Equal(winnerGain, loserLoss);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void ScoresOutsideZeroToOneAreRejected(double actualScore)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EloRating.Apply(1000, 1000, actualScore));
    }
}
