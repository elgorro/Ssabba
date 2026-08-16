using Ssabba.Domain;

namespace Ssabba.Domain.Tests;

/// <summary>
/// The scores are checked against the rules a match was played under, not against the official
/// ones. Half of these cases are therefore about <em>not</em> rejecting what a group really played.
/// </summary>
public class MatchScoringTests
{
    /// <summary>The FIVB 2v2 defaults: best of three, to 21, by two, deciding set to 15.</summary>
    private static string? Beach(params (int Home, int Away)[] sets) =>
        MatchScoring.Validate(Sets(sets), setsToWin: 2, pointsPerSet: 21, winBy: 2, tiebreakPoints: 15);

    private static MatchScoring.Set[] Sets((int Home, int Away)[] sets) =>
        [.. sets.Select((s, i) => new MatchScoring.Set(i + 1, s.Home, s.Away))];

    [Fact]
    public void A_straight_sets_win_is_plausible() =>
        Assert.Null(Beach((21, 18), (21, 15)));

    [Fact]
    public void A_deuce_is_plausible() =>
        Assert.Null(Beach((23, 21), (21, 19)));

    [Fact]
    public void A_deciding_set_is_played_to_the_tiebreak_target() =>
        Assert.Null(Beach((21, 18), (19, 21), (15, 13)));

    [Fact]
    public void Losing_the_first_set_and_the_match_is_plausible() =>
        Assert.Null(Beach((18, 21), (16, 21)));

    [Fact]
    public void A_set_that_went_past_the_target_must_end_on_the_margin()
    {
        // It was over at 23-21; 25-21 is a digit that slipped.
        var complaint = Beach((25, 21));

        Assert.NotNull(complaint);
        Assert.Contains("exactly 2", complaint);
    }

    [Fact]
    public void A_level_set_is_refused()
    {
        var complaint = Beach((21, 21));

        Assert.NotNull(complaint);
        Assert.Contains("level", complaint);
    }

    [Fact]
    public void A_negative_score_is_refused() =>
        Assert.NotNull(Beach((21, -5)));

    [Fact]
    public void A_set_won_short_of_the_target_is_refused()
    {
        var complaint = Beach((19, 17));

        Assert.NotNull(complaint);
        Assert.Contains("played to 21", complaint);
    }

    [Fact]
    public void A_one_point_win_is_refused() =>
        Assert.NotNull(Beach((21, 20)));

    [Fact]
    public void A_fourth_set_in_a_best_of_three_is_refused()
    {
        var complaint = Beach((21, 18), (19, 21), (15, 13), (21, 19));

        Assert.NotNull(complaint);
        Assert.Contains("at most 3 sets", complaint);
    }

    [Fact]
    public void A_set_played_after_the_match_was_won_is_refused()
    {
        var complaint = Beach((21, 18), (21, 15), (21, 19));

        Assert.NotNull(complaint);
        Assert.Contains("already won", complaint);
    }

    [Fact]
    public void A_match_without_sets_is_refused() =>
        Assert.NotNull(Beach());

    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 2)]
    public void Sets_must_be_numbered_from_one_without_gaps(int first, int second)
    {
        var complaint = MatchScoring.Validate(
            [new MatchScoring.Set(first, 21, 18), new MatchScoring.Set(second, 21, 15)],
            setsToWin: 2, pointsPerSet: 21, winBy: 2, tiebreakPoints: 15);

        Assert.NotNull(complaint);
        Assert.Contains("numbered from 1", complaint);
    }

    [Fact]
    public void A_house_rule_of_one_set_straight_to_21_accepts_a_single_point_win() =>
        Assert.Null(MatchScoring.Validate(
            [new MatchScoring.Set(1, 21, 20)],
            setsToWin: 1, pointsPerSet: 21, winBy: 0, tiebreakPoints: 0));

    [Fact]
    public void A_house_rule_of_one_set_to_15_refuses_a_set_stopped_early() =>
        Assert.NotNull(MatchScoring.Validate(
            [new MatchScoring.Set(1, 11, 9)],
            setsToWin: 1, pointsPerSet: 15, winBy: 2, tiebreakPoints: 0));

    [Fact]
    public void An_unfinished_match_is_plausible_but_not_decided()
    {
        var sets = Sets([(21, 18)]);

        Assert.Null(Beach((21, 18)));
        Assert.False(MatchScoring.IsDecided(sets, setsToWin: 2));
    }

    [Fact]
    public void A_match_taken_in_straight_sets_is_decided() =>
        Assert.True(MatchScoring.IsDecided(Sets([(21, 18), (21, 15)]), setsToWin: 2));
}
