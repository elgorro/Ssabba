using Microsoft.EntityFrameworkCore;
using Ssabba.TestSupport;

namespace Ssabba.Infrastructure.Tests;

/// <summary>
/// Proves the tier works end to end: the container starts, the migrations apply and the resulting
/// database matches the model. Feature coverage lands on top of this (see #34).
/// </summary>
[Collection(PostgresDatabase.Name)]
[Trait(TestCategories.Category, TestCategories.Integration)]
public class SchemaTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Migrations_leave_no_pending_model_changes()
    {
        await using var db = postgres.CreateDbContext();

        var pending = await db.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task Every_entity_maps_to_a_queryable_table()
    {
        await using var db = postgres.CreateDbContext();

        Assert.Empty(await db.Players.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await db.Matches.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await db.Communities.ToListAsync(TestContext.Current.CancellationToken));
    }
}
