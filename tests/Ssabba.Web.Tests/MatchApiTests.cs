using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Shared;
using Ssabba.TestSupport;

namespace Ssabba.Web.Tests;

/// <summary>
/// Matches end to end: who may write, that an impossible score is refused in words rather than
/// accepted quietly, and that correcting or striking a result puts back the rating it took (#18).
/// The chained recalculation a later match would need is #24 and is not claimed here.
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class MatchApiTests(PostgresFixture postgres) : IAsyncLifetime
{
    private SsabbaWebApplicationFactory factory = null!;

    public async ValueTask InitializeAsync()
    {
        await postgres.ResetAsync(TestContext.Current.CancellationToken);

        factory = new SsabbaWebApplicationFactory(postgres);
    }

    [Fact]
    public async Task Listing_matches_is_open_to_anyone()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ApiRoutes.Matches, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Creating_a_match_requires_a_signed_in_user()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches,
            Request(Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Editing_a_match_requires_a_signed_in_user()
    {
        await SeedAsync();

        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"{ApiRoutes.Matches}/{Guid.NewGuid()}",
            new UpdateMatchRequest(DateTimeOffset.UtcNow, null, null, Guid.NewGuid(), Guid.NewGuid(), []),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_match_requires_a_signed_in_user()
    {
        await SeedAsync();

        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            $"{ApiRoutes.Matches}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_unknown_match_is_not_found()
    {
        await SeedAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"{ApiRoutes.Matches}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_recorded_match_comes_back_with_its_sets_and_its_winner()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);
        var id = await RecordAsync(client, home, away, (21, 18), (21, 15));

        var match = await client.GetFromJsonAsync<MatchDetail>(
            $"{ApiRoutes.Matches}/{id}", TestContext.Current.CancellationToken);

        Assert.NotNull(match);
        Assert.Equal(2, match.Sets.Count);
        Assert.Equal(21, match.Sets[0].HomePoints);
        Assert.Equal(2, match.HomeSetsWon);
        Assert.Equal(0, match.AwaySetsWon);
        // Never entered, always read off the sets.
        Assert.Equal("HomeWin", match.Outcome);
        // The rules it was played under travel with it, so the form can check the same ones.
        Assert.Equal(21, match.PointsPerSet);
        Assert.Equal(15, match.TiebreakPoints);
    }

    [Theory]
    // A set nobody won.
    [InlineData(21, 21, "level")]
    // Stopped short of 21.
    [InlineData(19, 17, "played to 21")]
    // Won by one when two clear points are needed.
    [InlineData(21, 20, "clear points")]
    // Past 21, so it was over at 23-21 — a digit slipped.
    [InlineData(25, 21, "exactly 2")]
    public async Task An_impossible_score_is_refused_in_words(int home, int away, string because)
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (homeTeam, awayTeam) = await TwoTeamsAsync(client);

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches,
            Request(homeTeam, awayTeam, (home, away), (home, away)),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            because,
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_unfinished_match_cannot_be_recorded()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);

        // One set of a best of three: real, but not a result anybody can be rated on yet.
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches, Request(home, away, (21, 18)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "not finished",
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_team_cannot_play_itself()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, _) = await TwoTeamsAsync(client);

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches,
            Request(home, home, (21, 18), (21, 15)),
            TestContext.Current.CancellationToken);

        // A caller's mistake, answered as one rather than as a fault.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Correcting_a_score_leaves_the_ratings_where_recording_it_right_would_have()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);

        // What it should have said, recorded and then taken away again, to learn the target.
        var reference = await RecordAsync(client, home, away, (18, 21), (16, 21));
        var expected = await RatingsAsync();

        await client.DeleteAsync($"{ApiRoutes.Matches}/{reference}", TestContext.Current.CancellationToken);

        // Now the same match entered the wrong way round, then corrected.
        var id = await RecordAsync(client, home, away, (21, 18), (21, 16));

        var response = await client.PutAsJsonAsync(
            $"{ApiRoutes.Matches}/{id}",
            new UpdateMatchRequest(
                DateTimeOffset.UtcNow, null, null, home, away,
                [new SetScore(1, 18, 21), new SetScore(2, 16, 21)]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(expected, await RatingsAsync());

        var corrected = await client.GetFromJsonAsync<MatchDetail>(
            $"{ApiRoutes.Matches}/{id}", TestContext.Current.CancellationToken);

        Assert.NotNull(corrected);
        Assert.Equal("AwayWin", corrected.Outcome);
        Assert.Equal(2, corrected.Sets.Count);
    }

    [Fact]
    public async Task An_impossible_correction_is_refused_and_changes_nothing()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);
        var id = await RecordAsync(client, home, away, (21, 18), (21, 15));

        var before = await RatingsAsync();

        var response = await client.PutAsJsonAsync(
            $"{ApiRoutes.Matches}/{id}",
            new UpdateMatchRequest(
                DateTimeOffset.UtcNow, null, null, home, away,
                [new SetScore(1, 21, 21), new SetScore(2, 21, 15)]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        // The rating was never unwound, because the score was refused on the way in.
        Assert.Equal(before, await RatingsAsync());
    }

    [Fact]
    public async Task Deleting_a_match_hands_back_the_rating_it_took_and_strikes_it_from_the_lists()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);

        var untouched = await RatingsAsync();

        var id = await RecordAsync(client, home, away, (21, 18), (21, 15));

        Assert.NotEqual(untouched, await RatingsAsync());

        var response = await client.DeleteAsync(
            $"{ApiRoutes.Matches}/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(untouched, await RatingsAsync());

        // Struck from the record, not erased: the row stays, filtered out of every read.
        var listed = await client.GetFromJsonAsync<List<MatchSummary>>(
            ApiRoutes.Matches, TestContext.Current.CancellationToken) ?? [];

        Assert.DoesNotContain(listed, m => m.Id == id);

        var detail = await client.GetAsync($"{ApiRoutes.Matches}/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);

        await using var db = postgres.CreateDbContext();

        var struck = await db.Matches.SingleAsync(m => m.Id == id, TestContext.Current.CancellationToken);

        Assert.Equal(MatchStatus.Voided, struck.Status);
        Assert.NotNull(struck.DeletedAt);
        Assert.Null(struck.RatingAppliedAt);
        Assert.Empty(await db.MatchAppearances.Where(a => a.MatchId == id)
            .ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task The_teams_in_a_recorded_match_cannot_be_swapped_by_editing_it()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);
        var id = await RecordAsync(client, home, away, (21, 18), (21, 15));

        // Turning it round would have to move two other people's ratings, so it is refused rather
        // than quietly ignored.
        var response = await client.PutAsJsonAsync(
            $"{ApiRoutes.Matches}/{id}",
            new UpdateMatchRequest(
                DateTimeOffset.UtcNow, null, null, away, home,
                [new SetScore(1, 21, 18), new SetScore(2, 21, 15)]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_match_twice_is_not_found_the_second_time()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);
        var id = await RecordAsync(client, home, away, (21, 18), (21, 15));

        await client.DeleteAsync($"{ApiRoutes.Matches}/{id}", TestContext.Current.CancellationToken);

        var again = await client.DeleteAsync($"{ApiRoutes.Matches}/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
    }

    [Fact]
    public async Task The_match_page_shows_the_teams_and_the_set_scores()
    {
        await SeedAsync();

        using var client = factory.CreateClientAs("ada");

        var (home, away) = await TwoTeamsAsync(client);
        var id = await RecordAsync(client, home, away, (21, 18), (21, 15));

        using var anyone = factory.CreateClient();

        var page = await anyone.GetAsync($"/matches/{id}", TestContext.Current.CancellationToken);

        page.EnsureSuccessStatusCode();

        var html = await page.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Ada Lovelace", html, StringComparison.Ordinal);
        Assert.Contains("Grace Hopper", html, StringComparison.Ordinal);
        Assert.Contains("18", html, StringComparison.Ordinal);
    }

    private static CreateMatchRequest Request(
        Guid homeTeamId,
        Guid awayTeamId,
        params (int Home, int Away)[] sets) =>
        new(DateTimeOffset.UtcNow,
            CourtId: null,
            LocationNote: null,
            homeTeamId,
            awayTeamId,
            [.. sets.Select((s, i) => new SetScore(i + 1, s.Home, s.Away))]);

    private static async Task<Guid> RecordAsync(
        HttpClient client,
        Guid homeTeamId,
        Guid awayTeamId,
        params (int Home, int Away)[] sets)
    {
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches, Request(homeTeamId, awayTeamId, sets), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    /// <summary>Every member's rating, keyed by player, so a whole ladder can be compared at once.</summary>
    private async Task<Dictionary<Guid, int>> RatingsAsync()
    {
        await using var db = postgres.CreateDbContext();

        return await db.CommunityMembers.AsNoTracking()
            .ToDictionaryAsync(m => m.PlayerId, m => m.Rating, TestContext.Current.CancellationToken);
    }

    private static async Task<(Guid Home, Guid Away)> TwoTeamsAsync(HttpClient client) =>
        (await TeamAsync(client, "Ada Lovelace", "Grace Hopper"),
            await TeamAsync(client, "Alan Turing", "Edsger Dijkstra"));

    private static async Task<Guid> TeamAsync(HttpClient client, params string[] names)
    {
        var playerIds = new List<Guid>();

        foreach (var name in names)
        {
            var created = await client.PostAsJsonAsync(
                ApiRoutes.Players,
                new CreatePlayerRequest(
                    name, Slug: null, PreferredTimeZone: null, Locale: null,
                    Nickname: null, Role: null, Profile: null),
                TestContext.Current.CancellationToken);

            created.EnsureSuccessStatusCode();

            playerIds.Add(await created.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken));
        }

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Teams,
            new CreateTeamRequest(null, true, playerIds),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// A community and the 2v2 format. Both are seeded by migrations, and resetting the database
    /// between tests truncates them along with everything else.
    /// </summary>
    private async Task SeedAsync()
    {
        await using var db = postgres.CreateDbContext();

        db.Communities.Add(new Community { Name = "Tuesday round", Slug = "tuesday-round" });

        if (!await db.Formats.AnyAsync(f => f.PlayersPerSide == 2, TestContext.Current.CancellationToken))
        {
            db.Formats.Add(new Format
            {
                Code = FormatCode.TwoVsTwo,
                PlayersPerSide = 2,
                Name = "2v2",
                DefaultSetsToWin = 2,
                DefaultPointsPerSet = 21,
                DefaultTiebreakPoints = 15,
            });
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
