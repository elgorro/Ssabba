using Ssabba.Domain.Entities;

namespace Ssabba.Domain.Tests;

public class MatchTests
{
    private static Match MatchWith(params (int Home, int Away)[] sets) => new()
    {
        PlayedAt = DateTimeOffset.UnixEpoch,
        HomeTeamId = Guid.CreateVersion7(),
        AwayTeamId = Guid.CreateVersion7(),
        Sets = [.. sets.Select((s, i) => new MatchSet { Number = i + 1, HomePoints = s.Home, AwayPoints = s.Away })],
    };

    [Fact]
    public void TwoSetsToNone_IsAHomeWin()
    {
        var match = MatchWith((21, 18), (21, 15));

        Assert.Equal(2, match.HomeSetsWon);
        Assert.Equal(0, match.AwaySetsWon);
        Assert.Equal(MatchOutcome.HomeWin, match.Outcome);
    }

    [Fact]
    public void LosingTheDecider_IsAnAwayWin()
    {
        var match = MatchWith((21, 18), (19, 21), (13, 15));

        Assert.Equal(MatchOutcome.AwayWin, match.Outcome);
    }

    [Fact]
    public void AMatchWithoutSets_IsUndecided()
    {
        Assert.Equal(MatchOutcome.Undecided, MatchWith().Outcome);
    }
}
