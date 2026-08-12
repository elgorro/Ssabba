using System.Net;
using System.Net.Http.Json;
using Ssabba.Shared;
using Ssabba.TestSupport;

namespace Ssabba.Web.Tests;

/// <summary>
/// Proves the API tier works end to end: the real host boots against the real database and the
/// authorization on the write path is enforced. Behaviour coverage lands on top of this.
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class MatchApiTests(PostgresFixture postgres) : IAsyncLifetime
{
    private SsabbaWebApplicationFactory factory = null!;

    public ValueTask InitializeAsync()
    {
        factory = new SsabbaWebApplicationFactory(postgres);

        return ValueTask.CompletedTask;
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
            new CreateMatchRequest(DateTimeOffset.UtcNow, null, null, Guid.NewGuid(), Guid.NewGuid(), []),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_signed_in_user_gets_past_authorization()
    {
        using var client = factory.CreateClientAs("ada");

        var response = await client.PostAsJsonAsync(
            ApiRoutes.Matches,
            new CreateMatchRequest(DateTimeOffset.UtcNow, null, null, Guid.NewGuid(), Guid.NewGuid(), []),
            TestContext.Current.CancellationToken);

        // The teams do not exist, so this fails on the way in — but not on authentication.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public ValueTask DisposeAsync()
    {
        factory.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
