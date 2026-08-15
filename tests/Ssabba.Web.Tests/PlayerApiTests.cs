using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Shared;
using Ssabba.TestSupport;

namespace Ssabba.Web.Tests;

/// <summary>
/// The roster end to end: who may write, what a retired player does to the list, and the community
/// filter every query owes (ADR-0002, issue #44).
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class PlayerApiTests(PostgresFixture postgres) : IAsyncLifetime
{
    private SsabbaWebApplicationFactory factory = null!;

    public async ValueTask InitializeAsync()
    {
        await postgres.ResetAsync(TestContext.Current.CancellationToken);

        factory = new SsabbaWebApplicationFactory(postgres);
    }

    [Fact]
    public async Task Listing_players_is_open_to_anyone()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Adding_a_player_requires_a_signed_in_user()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Players,
            Request("Ada Lovelace"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Retiring_a_player_requires_a_signed_in_user()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            $"{ApiRoutes.Players}/{Guid.NewGuid()}/retire",
            content: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_added_player_appears_on_the_roster_with_a_derived_slug()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var created = await client.PostAsJsonAsync(
            ApiRoutes.Players,
            Request("Jürgen Müller"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var players = await ListAsync(client);

        var player = Assert.Single(players);
        Assert.Equal("Jürgen Müller", player.DisplayName);
        Assert.Equal("jurgen-muller", player.Slug);
        Assert.Equal("Member", player.Role);
        Assert.False(player.IsRetired);
        Assert.Equal(EloRating.InitialRating, player.Rating);
    }

    [Fact]
    public async Task A_second_player_cannot_take_a_slug_already_in_use()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        await client.PostAsJsonAsync(ApiRoutes.Players, Request("Ada Lovelace"), TestContext.Current.CancellationToken);

        var second = await client.PostAsJsonAsync(
            ApiRoutes.Players,
            Request("ada lovelace"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Single(await ListAsync(client));
    }

    [Fact]
    public async Task Editing_a_player_updates_the_identity_the_membership_and_the_profile()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var id = await CreateAsync(client, "Ada Lovelace");

        var update = await client.PutAsJsonAsync(
            $"{ApiRoutes.Players}/{id}",
            new UpdatePlayerRequest(
                "Ada King",
                Slug: null,
                PreferredTimeZone: "Europe/London",
                Locale: "en-GB",
                Nickname: "Countess",
                Role: "Organizer",
                Profile: new PlayerProfileDto(172, ["Setter", "Defender"], 7, 1980, "Counts well.", IsLeftHanded: true)),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var player = await client.GetFromJsonAsync<PlayerDetail>(
            $"{ApiRoutes.Players}/{id}", TestContext.Current.CancellationToken);

        Assert.NotNull(player);
        Assert.Equal("Ada King", player.DisplayName);
        Assert.Equal("ada-king", player.Slug);
        Assert.Equal("Countess", player.Nickname);
        Assert.Equal("Organizer", player.Role);
        Assert.Equal("Europe/London", player.PreferredTimeZone);
        Assert.Equal(172, player.Profile.HeightCm);
        Assert.Equal(["Defender", "Setter"], [.. player.Profile.PreferredPositions.Order()]);
        Assert.True(player.Profile.IsLeftHanded);
    }

    [Fact]
    public async Task Retiring_a_player_hides_them_from_the_roster_but_keeps_their_record()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var id = await CreateAsync(client, "Ada Lovelace");
        await GiveThemAHistoryAsync(id);

        var retire = await client.PostAsync(
            $"{ApiRoutes.Players}/{id}/retire", content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, retire.StatusCode);

        Assert.Empty(await ListAsync(client));

        var retired = Assert.Single(await ListAsync(client, includeRetired: true));
        Assert.True(retired.IsRetired);

        // The whole point: retiring is a membership status, not an erasure.
        Assert.Equal(1234, retired.Rating);
        Assert.Equal(7, retired.MatchesPlayed);

        await using var db = postgres.CreateDbContext();
        var membership = await db.CommunityMembers.SingleAsync(
            m => m.PlayerId == id, TestContext.Current.CancellationToken);

        Assert.Equal(MembershipStatus.Left, membership.Status);
        Assert.NotNull(membership.LeftAt);
        Assert.Null(await db.Players.Where(p => p.Id == id).Select(p => p.DeletedAt)
            .SingleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task A_reinstated_player_is_back_on_the_roster()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var id = await CreateAsync(client, "Ada Lovelace");

        await client.PostAsync($"{ApiRoutes.Players}/{id}/retire", content: null, TestContext.Current.CancellationToken);
        await client.PostAsync($"{ApiRoutes.Players}/{id}/reinstate", content: null, TestContext.Current.CancellationToken);

        var player = Assert.Single(await ListAsync(client));

        Assert.False(player.IsRetired);
    }

    [Fact]
    public async Task A_player_of_another_community_never_shows_up_on_this_roster()
    {
        var communityId = await SeedCommunityAsync();
        var otherCommunityId = await SeedCommunityAsync("Other beach", "other-beach");

        using var client = factory.CreateClientAs("ada");

        var id = await CreateAsync(client, "Ada Lovelace");

        // The endpoint resolves the oldest community, so the roster it serves is the first one.
        Assert.NotEqual(communityId, otherCommunityId);

        await using (var db = postgres.CreateDbContext())
        {
            db.Players.Add(new Player { DisplayName = "Grace Hopper", Slug = "grace-hopper" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var grace = await db.Players.SingleAsync(p => p.Slug == "grace-hopper", TestContext.Current.CancellationToken);
            db.CommunityMembers.Add(new CommunityMember { CommunityId = otherCommunityId, PlayerId = grace.Id });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var players = await ListAsync(client, includeRetired: true);

        Assert.Equal(id, Assert.Single(players).Id);
    }

    [Fact]
    public async Task An_unknown_player_is_a_404()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"{ApiRoutes.Players}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Without_a_community_the_roster_is_a_404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task The_roster_pages_render()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        await CreateAsync(client, "Ada Lovelace");

        var list = await client.GetAsync("/players", TestContext.Current.CancellationToken);
        var detail = await client.GetAsync("/players/ada-lovelace", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Contains("Ada Lovelace", await detail.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static CreatePlayerRequest Request(string displayName) =>
        new(displayName, Slug: null, PreferredTimeZone: null, Locale: null, Nickname: null, Role: null, Profile: null);

    private static async Task<Guid> CreateAsync(HttpClient client, string displayName)
    {
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Players, Request(displayName), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private static async Task<List<PlayerSummary>> ListAsync(HttpClient client, bool includeRetired = false)
    {
        var url = includeRetired ? $"{ApiRoutes.Players}?includeRetired=true" : ApiRoutes.Players;

        return await client.GetFromJsonAsync<List<PlayerSummary>>(url, TestContext.Current.CancellationToken) ?? [];
    }

    private async Task<Guid> SeedCommunityAsync(string name = "Tuesday round", string slug = "tuesday-round")
    {
        await using var db = postgres.CreateDbContext();

        var community = new Community { Name = name, Slug = slug };

        db.Communities.Add(community);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return community.Id;
    }

    /// <summary>Gives the membership something to lose, so retiring can be shown not to lose it.</summary>
    private async Task GiveThemAHistoryAsync(Guid playerId)
    {
        await using var db = postgres.CreateDbContext();

        var membership = await db.CommunityMembers.SingleAsync(
            m => m.PlayerId == playerId, TestContext.Current.CancellationToken);

        membership.Rating = 1234;
        membership.MatchesPlayed = 7;

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
