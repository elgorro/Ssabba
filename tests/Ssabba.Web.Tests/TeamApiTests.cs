using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ssabba.Domain;
using Ssabba.Domain.Entities;
using Ssabba.Shared;
using Ssabba.TestSupport;

namespace Ssabba.Web.Tests;

/// <summary>
/// Teams end to end: who may write, that a lineup already on record is picked up rather than
/// duplicated (issue #17), and the community filter every query owes (ADR-0002).
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class TeamApiTests(PostgresFixture postgres) : IAsyncLifetime
{
    private SsabbaWebApplicationFactory factory = null!;

    public async ValueTask InitializeAsync()
    {
        await postgres.ResetAsync(TestContext.Current.CancellationToken);

        factory = new SsabbaWebApplicationFactory(postgres);
    }

    [Fact]
    public async Task Listing_teams_is_open_to_anyone()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(ApiRoutes.Teams, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Forming_a_team_requires_a_signed_in_user()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, [Guid.NewGuid(), Guid.NewGuid()]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_team_requires_a_signed_in_user()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            $"{ApiRoutes.Teams}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_new_lineup_is_created_and_listed_under_its_members_names()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, [ada, grace]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var team = Assert.Single(await ListAsync(client));
        Assert.Equal("Ada Lovelace / Grace Hopper", team.DisplayName);
        Assert.True(team.IsAdHoc);
        Assert.Equal(0, team.MatchesPlayed);
    }

    [Fact]
    public async Task The_same_lineup_entered_twice_is_the_same_team()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");

        var first = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, [ada, grace]),
            TestContext.Current.CancellationToken);

        // Entered the other way round: the roster is the key, not the order it was typed in.
        var second = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, [grace, ada]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(await IdOf(first), await IdOf(second));
        Assert.Single(await ListAsync(client));
    }

    [Fact]
    public async Task Naming_a_lineup_that_already_played_promotes_it_instead_of_copying_it()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");

        var adHoc = await CreateTeamAsync(client, [ada, grace]);

        var named = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest("The Analysts", IsAdHoc: false, [ada, grace]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, named.StatusCode);
        Assert.Equal(adHoc, await IdOf(named));

        var team = Assert.Single(await ListAsync(client));
        Assert.Equal("The Analysts", team.DisplayName);
        Assert.False(team.IsAdHoc);
    }

    [Fact]
    public async Task Standing_only_hides_the_one_off_lineups()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");
        var alan = await AddPlayerAsync(client, "Alan Turing");

        await CreateTeamAsync(client, [ada, grace]);
        await CreateTeamAsync(client, [ada, alan], name: "The Analysts", isAdHoc: false);

        var standing = Assert.Single(await ListAsync(client, standingOnly: true));

        Assert.Equal("The Analysts", standing.DisplayName);
        Assert.Equal(2, (await ListAsync(client)).Count);
    }

    [Fact]
    public async Task A_team_needs_at_least_two_players_who_belong_here()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");

        var alone = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, [ada]),
            TestContext.Current.CancellationToken);

        var stranger = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, [ada, Guid.NewGuid()]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, alone.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, stranger.StatusCode);
        Assert.Empty(await ListAsync(client));
    }

    [Fact]
    public async Task Editing_a_team_onto_another_teams_roster_is_refused()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");
        var alan = await AddPlayerAsync(client, "Alan Turing");

        await CreateTeamAsync(client, [ada, grace], name: "The Analysts");
        var other = await CreateTeamAsync(client, [ada, alan]);

        var response = await client.PutAsJsonAsync(
            $"{ApiRoutes.Teams}/{other}",
            new UpdateTeamRequest(null, true, [grace, ada]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "The Analysts",
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Editing_a_team_swaps_a_player_and_re_keys_it()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");
        var alan = await AddPlayerAsync(client, "Alan Turing");

        var id = await CreateTeamAsync(client, [ada, grace]);

        var response = await client.PutAsJsonAsync(
            $"{ApiRoutes.Teams}/{id}",
            new UpdateTeamRequest("The Analysts", IsAdHoc: false, [alan, ada]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var team = await client.GetFromJsonAsync<TeamDetail>(
            $"{ApiRoutes.Teams}/{id}", TestContext.Current.CancellationToken);

        Assert.NotNull(team);
        Assert.Equal(["Alan Turing", "Ada Lovelace"], team.Members.Select(m => m.DisplayName));
        Assert.False(team.IsAdHoc);

        await using var db = postgres.CreateDbContext();
        var stored = await db.Teams.SingleAsync(t => t.Id == id, TestContext.Current.CancellationToken);
        Assert.Equal(TeamRoster.Key([alan, ada]), stored.MemberKey);
    }

    [Fact]
    public async Task A_team_that_never_played_can_be_deleted()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");

        var id = await CreateTeamAsync(client, [ada, grace]);

        var response = await client.DeleteAsync($"{ApiRoutes.Teams}/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await ListAsync(client));
    }

    [Fact]
    public async Task A_teams_record_counts_wins_from_both_sides_of_the_net()
    {
        await SeedCommunityAsync();
        await SeedBeachFormatAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");
        var alan = await AddPlayerAsync(client, "Alan Turing");
        var edsger = await AddPlayerAsync(client, "Edsger Dijkstra");

        var ours = await CreateTeamAsync(client, [ada, grace], name: "The Analysts");
        var theirs = await CreateTeamAsync(client, [alan, edsger]);

        // Won at home, then won away, then lost at home.
        await RecordMatchAsync(client, ours, theirs, homePoints: 21, awayPoints: 15);
        await RecordMatchAsync(client, theirs, ours, homePoints: 12, awayPoints: 21);
        await RecordMatchAsync(client, ours, theirs, homePoints: 18, awayPoints: 21);

        var team = await client.GetFromJsonAsync<TeamDetail>(
            $"{ApiRoutes.Teams}/{ours}", TestContext.Current.CancellationToken);

        Assert.NotNull(team);
        Assert.Equal(2, team.Wins);
        Assert.Equal(1, team.Losses);
        Assert.Equal(3, team.MatchesPlayed);

        var matches = await client.GetFromJsonAsync<List<TeamMatchRow>>(
            $"{ApiRoutes.Teams}/{ours}/matches", TestContext.Current.CancellationToken);

        Assert.NotNull(matches);
        Assert.Equal(3, matches.Count);
        Assert.All(matches, m => Assert.Equal("Alan Turing / Edsger Dijkstra", m.Opponent));
        Assert.Equal(2, matches.Count(m => m.Won));
    }

    [Fact]
    public async Task A_team_that_has_played_cannot_be_deleted()
    {
        await SeedCommunityAsync();
        await SeedBeachFormatAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");
        var alan = await AddPlayerAsync(client, "Alan Turing");
        var edsger = await AddPlayerAsync(client, "Edsger Dijkstra");

        var ours = await CreateTeamAsync(client, [ada, grace]);
        var theirs = await CreateTeamAsync(client, [alan, edsger]);

        await RecordMatchAsync(client, ours, theirs, homePoints: 21, awayPoints: 15);

        var response = await client.DeleteAsync($"{ApiRoutes.Teams}/{ours}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "1 match",
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task A_team_of_another_community_never_shows_up_here()
    {
        await SeedCommunityAsync();
        var otherCommunityId = await SeedCommunityAsync("Other beach", "other-beach");

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");

        await CreateTeamAsync(client, [ada, grace]);

        Guid strangerId;

        await using (var db = postgres.CreateDbContext())
        {
            var stranger = new Team { CommunityId = otherCommunityId, Name = "Elsewhere", MemberKey = "elsewhere" };

            db.Teams.Add(stranger);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            strangerId = stranger.Id;
        }

        var teams = await ListAsync(client);
        var detail = await client.GetAsync($"{ApiRoutes.Teams}/{strangerId}", TestContext.Current.CancellationToken);

        Assert.Single(teams);
        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);
    }

    [Fact]
    public async Task An_unknown_team_is_a_404()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"{ApiRoutes.Teams}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task The_team_pages_render()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var ada = await AddPlayerAsync(client, "Ada Lovelace");
        var grace = await AddPlayerAsync(client, "Grace Hopper");

        var id = await CreateTeamAsync(client, [ada, grace], name: "The Analysts");

        var list = await client.GetAsync("/teams", TestContext.Current.CancellationToken);
        var detail = await client.GetAsync($"/teams/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Contains(
            "The Analysts",
            await detail.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static async Task<Guid> AddPlayerAsync(HttpClient client, string displayName)
    {
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Players,
            new CreatePlayerRequest(
                displayName, Slug: null, PreferredTimeZone: null, Locale: null,
                Nickname: null, Role: null, Profile: null),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private static async Task<Guid> CreateTeamAsync(
        HttpClient client,
        IReadOnlyList<Guid> playerIds,
        string? name = null,
        bool isAdHoc = true)
    {
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(name, isAdHoc, playerIds),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await IdOf(response);
    }

    private static async Task RecordMatchAsync(
        HttpClient client,
        Guid homeTeamId,
        Guid awayTeamId,
        int homePoints,
        int awayPoints)
    {
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches,
            new CreateMatchRequest(
                DateTimeOffset.UtcNow,
                CourtId: null,
                LocationNote: null,
                homeTeamId,
                awayTeamId,
                [new SetScore(1, homePoints, awayPoints)]),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static Task<Guid> IdOf(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

    private static async Task<List<TeamSummary>> ListAsync(HttpClient client, bool standingOnly = false)
    {
        var url = standingOnly ? $"{ApiRoutes.Teams}?standingOnly=true" : ApiRoutes.Teams;

        return await client.GetFromJsonAsync<List<TeamSummary>>(url, TestContext.Current.CancellationToken) ?? [];
    }

    /// <summary>
    /// Puts 2v2 back: the formats are seeded by a migration, and resetting the database between
    /// tests truncates them along with everything else.
    /// </summary>
    private async Task SeedBeachFormatAsync()
    {
        await using var db = postgres.CreateDbContext();

        if (await db.Formats.AnyAsync(f => f.PlayersPerSide == 2, TestContext.Current.CancellationToken))
        {
            return;
        }

        db.Formats.Add(new Format
        {
            Code = FormatCode.TwoVsTwo,
            PlayersPerSide = 2,
            Name = "2v2",
            DefaultSetsToWin = 2,
            DefaultPointsPerSet = 21,
            DefaultTiebreakPoints = 15,
        });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<Guid> SeedCommunityAsync(string name = "Tuesday round", string slug = "tuesday-round")
    {
        await using var db = postgres.CreateDbContext();

        var community = new Community { Name = name, Slug = slug };

        db.Communities.Add(community);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return community.Id;
    }

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
