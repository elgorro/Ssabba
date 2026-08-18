using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Shared;
using Ssabba.TestSupport;

namespace Ssabba.Web.Tests;

/// <summary>
/// Signing in and being someone: the player row a subject gets, what it is not allowed to take over,
/// and the fact that the app can now name who it is acting as.
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class CurrentPlayerTests(PostgresFixture postgres) : IAsyncLifetime
{
    private SsabbaWebApplicationFactory factory = null!;

    public async ValueTask InitializeAsync()
    {
        await postgres.ResetAsync(TestContext.Current.CancellationToken);

        factory = new SsabbaWebApplicationFactory(postgres);
    }

    [Fact]
    public async Task Signing_in_creates_a_player_for_the_subject()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        var player = await SinglePlayerAsync();

        Assert.Equal(TestAuthHandler.SubjectFor("ada"), player.SubjectId);
        Assert.Equal("ada", player.DisplayName);
        Assert.Equal("ada", player.Slug);
    }

    [Fact]
    public async Task Signing_in_again_reuses_the_same_player()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);
        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        await using var db = postgres.CreateDbContext();

        Assert.Equal(1, await db.Players.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task A_new_account_never_takes_over_a_player_entered_by_hand()
    {
        var communityId = await SeedCommunityAsync();
        var byHand = await SeedPlayerAsync(communityId, "Ada Lovelace", "ada", rating: 1400);

        using var client = factory.CreateClientAs("ada");

        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        await using var db = postgres.CreateDbContext();

        var untouched = await db.Players.SingleAsync(p => p.Id == byHand, TestContext.Current.CancellationToken);
        Assert.Null(untouched.SubjectId);

        // The signed-in player is a second, distinct row — and the slug it wanted was taken.
        var signedIn = await db.Players.SingleAsync(
            p => p.SubjectId == TestAuthHandler.SubjectFor("ada"),
            TestContext.Current.CancellationToken);

        Assert.NotEqual(byHand, signedIn.Id);
        Assert.Equal("ada-2", signedIn.Slug);
    }

    [Fact]
    public async Task A_signed_in_player_joins_the_community_as_a_member()
    {
        var communityId = await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        await using var db = postgres.CreateDbContext();

        var membership = await db.CommunityMembers.SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(communityId, membership.CommunityId);
        Assert.Equal(CommunityRole.Member, membership.Role);
    }

    [Fact]
    public async Task An_instance_with_no_community_still_gets_a_player()
    {
        using var client = factory.CreateClientAs("ada");

        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        await using var db = postgres.CreateDbContext();

        Assert.Equal(TestAuthHandler.SubjectFor("ada"), (await SinglePlayerAsync()).SubjectId);
        Assert.Empty(await db.CommunityMembers.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task The_signed_in_player_is_named_and_linked_in_the_page_chrome()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClientAs("ada");

        var html = await client.GetStringAsync("/players", TestContext.Current.CancellationToken);

        Assert.Contains("href=\"/players/ada\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_anonymous_request_is_nobody()
    {
        await SeedCommunityAsync();

        using var client = factory.CreateClient();

        await client.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        await using var db = postgres.CreateDbContext();

        Assert.Empty(await db.Players.ToListAsync(TestContext.Current.CancellationToken));
    }

    private async Task<Player> SinglePlayerAsync()
    {
        await using var db = postgres.CreateDbContext();

        return await db.Players.SingleAsync(TestContext.Current.CancellationToken);
    }

    private async Task<Guid> SeedCommunityAsync(string name = "Tuesday round", string slug = "tuesday-round")
    {
        await using var db = postgres.CreateDbContext();

        var community = new Community { Name = name, Slug = slug };

        db.Communities.Add(community);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return community.Id;
    }

    private async Task<Guid> SeedPlayerAsync(Guid communityId, string displayName, string slug, int rating)
    {
        await using var db = postgres.CreateDbContext();

        var player = new Player { DisplayName = displayName, Slug = slug };

        db.Players.Add(player);
        db.CommunityMembers.Add(new CommunityMember
        {
            CommunityId = communityId,
            PlayerId = player.Id,
            Rating = rating,
        });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return player.Id;
    }

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
