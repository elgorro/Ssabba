using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
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

    [Fact]
    public async Task The_same_lineup_cannot_be_two_teams_in_one_community()
    {
        await using var db = postgres.CreateDbContext();

        // Rolled back at the end: this assembly shares one database and the test above it proves the
        // tables are empty.
        await using var transaction = await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var community = new Community { Name = "Tuesday round", Slug = $"tuesday-{Guid.NewGuid():N}" };

        db.Communities.Add(community);
        db.Teams.Add(new Team { CommunityId = community.Id, MemberKey = "ada-grace" });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.Teams.Add(new Team { CommunityId = community.Id, MemberKey = "ada-grace" });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync(TestContext.Current.CancellationToken));

        await transaction.RollbackAsync(TestContext.Current.CancellationToken);
    }
}
