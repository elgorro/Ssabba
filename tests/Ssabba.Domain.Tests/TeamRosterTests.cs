namespace Ssabba.Domain.Tests;

public class TeamRosterTests
{
    private static readonly Guid Ada = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Grace = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Alan = new("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Key_does_not_depend_on_the_order_the_players_were_entered_in() =>
        Assert.Equal(TeamRoster.Key([Ada, Grace, Alan]), TeamRoster.Key([Alan, Ada, Grace]));

    [Fact]
    public void Key_ignores_a_player_listed_twice() =>
        Assert.Equal(TeamRoster.Key([Ada, Grace]), TeamRoster.Key([Ada, Grace, Ada]));

    [Fact]
    public void Key_tells_different_lineups_apart() =>
        Assert.NotEqual(TeamRoster.Key([Ada, Grace]), TeamRoster.Key([Ada, Alan]));

    [Fact]
    public void Key_is_the_ids_as_hex_sorted_and_hyphenated() =>
        Assert.Equal(
            "11111111111111111111111111111111-22222222222222222222222222222222",
            TeamRoster.Key([Grace, Ada]));

    [Fact]
    public void Key_of_nothing_is_empty() => Assert.Equal("", TeamRoster.Key([]));

    [Fact]
    public void A_full_side_of_six_still_fits_the_column() =>
        Assert.True(TeamRoster.Key(Enumerable.Range(0, 6).Select(_ => Guid.NewGuid())).Length <= TeamRoster.MaxKeyLength);
}
