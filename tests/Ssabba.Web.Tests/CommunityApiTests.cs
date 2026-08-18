using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Shared;
using Ssabba.TestSupport;
using Ssabba.Web.Endpoints;

namespace Ssabba.Web.Tests;

/// <summary>
/// First run: the one community this instance is for, and the one membership nobody granted.
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class CommunityApiTests(PostgresFixture postgres) : IAsyncLifetime
{
    private SsabbaWebApplicationFactory factory = null!;

    public async ValueTask InitializeAsync()
    {
        await postgres.ResetAsync(TestContext.Current.CancellationToken);

        factory = new SsabbaWebApplicationFactory(postgres);
    }

    [Fact]
    public async Task An_instance_without_a_community_has_nothing_to_show()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ApiRoutes.Community, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Setting_the_instance_up_makes_the_first_signer_its_owner()
    {
        using var client = factory.CreateClientAs("ada");

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Community,
            Request("Tuesday round"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CommunityDetail>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(created);
        Assert.Equal("Tuesday round", created.Name);
        Assert.Equal("tuesday-round", created.Slug);
        Assert.Equal("Europe/Zurich", created.TimeZone);
        Assert.Equal("CHF", created.Currency);

        await using var db = postgres.CreateDbContext();

        var membership = await db.CommunityMembers
            .Include(m => m.Player)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(created.Id, membership.CommunityId);
        Assert.Equal(CommunityRole.Owner, membership.Role);
        Assert.Equal(MembershipStatus.Active, membership.Status);
        Assert.Equal("ada", membership.Player!.DisplayName);
    }

    [Fact]
    public async Task An_instance_is_set_up_once()
    {
        using var client = factory.CreateClientAs("ada");

        await client.PostAsJsonAsync(
            ApiRoutes.Community,
            Request("Tuesday round"),
            TestContext.Current.CancellationToken);

        var second = await client.PostAsJsonAsync(
            ApiRoutes.Community,
            Request("Thursday round", slug: "thursday-round"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        await using var db = postgres.CreateDbContext();

        Assert.Equal(1, await db.Communities.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Everyone_who_signs_in_afterwards_is_a_member()
    {
        using var owner = factory.CreateClientAs("ada");
        await owner.PostAsJsonAsync(ApiRoutes.Community, Request("Tuesday round"), TestContext.Current.CancellationToken);

        using var latecomer = factory.CreateClientAs("grace");
        await latecomer.GetAsync(ApiRoutes.Players, TestContext.Current.CancellationToken);

        await using var db = postgres.CreateDbContext();

        var role = await db.CommunityMembers
            .Where(m => m.Player!.DisplayName == "grace")
            .Select(m => m.Role)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CommunityRole.Member, role);
    }

    [Fact]
    public async Task Anonymous_visitors_do_not_set_up_the_instance()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Community,
            Request("Tuesday round"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        await using var db = postgres.CreateDbContext();

        Assert.False(await db.Communities.AnyAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task The_owner_may_rename_the_community_and_the_public_identifier_survives()
    {
        using var client = factory.CreateClientAs("ada");

        var created = await (await client.PostAsJsonAsync(
            ApiRoutes.Community,
            Request("Tuesday round"),
            TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<CommunityDetail>(TestContext.Current.CancellationToken);

        var response = await client.PutAsJsonAsync(
            ApiRoutes.Community,
            new UpdateCommunityRequest("Thursday round", null, "Now on Thursdays", "Europe/Berlin", "eur", "Public"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var db = postgres.CreateDbContext();

        var community = await db.Communities.SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Thursday round", community.Name);
        Assert.Equal("thursday-round", community.Slug);
        Assert.Equal("EUR", community.Currency);
        Assert.Equal(CommunityVisibility.Public, community.Visibility);
        Assert.Equal(created!.PublicKeyId, community.PublicKeyId);
    }

    [Fact]
    public async Task A_member_may_not_rename_the_community()
    {
        using var owner = factory.CreateClientAs("ada");
        await owner.PostAsJsonAsync(ApiRoutes.Community, Request("Tuesday round"), TestContext.Current.CancellationToken);

        using var member = factory.CreateClientAs("grace");

        var response = await member.PutAsJsonAsync(
            ApiRoutes.Community,
            new UpdateCommunityRequest("Grace's round", null, null, null, null, null),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var db = postgres.CreateDbContext();

        Assert.Equal("Tuesday round", await db.Communities
            .Select(c => c.Name)
            .SingleAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// One instance, one community. A second row is a broken instance, and the resolver says so
    /// rather than picking one — see ADR-0002.
    /// </summary>
    [Fact]
    public async Task An_instance_holding_two_communities_refuses_to_guess()
    {
        await using var db = postgres.CreateDbContext();

        db.Communities.Add(new Community { Name = "Tuesday round", Slug = "tuesday-round" });
        db.Communities.Add(new Community { Name = "Thursday round", Slug = "thursday-round" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CommunityQueries.ResolveCommunityIdAsync(db, TestContext.Current.CancellationToken));
    }

    private static CreateCommunityRequest Request(string name, string? slug = null) =>
        new(name, slug, "The regulars", "Europe/Zurich", "chf", "Private");

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
