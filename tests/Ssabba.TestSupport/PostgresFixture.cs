using Microsoft.EntityFrameworkCore;
using Ssabba.Infrastructure;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ssabba.TestSupport;

/// <summary>
/// A single PostgreSQL container per test collection, migrated once. Tests run against the real
/// provider on purpose: the in-memory and SQLite providers disagree with Npgsql often enough that a
/// green suite would not mean much.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    // Matches the image the deployed stack runs.
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("ssabba")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync(TestContext.Current.CancellationToken);

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    public SsabbaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SsabbaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new SsabbaDbContext(options);
    }

    /// <summary>Empties every table so a test starts from a known state without paying for a new container.</summary>
    public async Task ResetAsync(CancellationToken ct = default)
    {
        await using var db = CreateDbContext();

        var tables = db.Model.GetEntityTypes()
            .Select(entity => (Schema: entity.GetSchema(), Table: entity.GetTableName()))
            .Where(t => t.Table is not null)
            .Distinct()
            .Select(t => $"\"{t.Schema ?? "public"}\".\"{t.Table}\"");

        // The table names come from the EF model, not from anything a test supplies.
#pragma warning disable EF1002
        await db.Database.ExecuteSqlRawAsync(
            $"TRUNCATE {string.Join(", ", tables)} RESTART IDENTITY CASCADE;", ct);
#pragma warning restore EF1002
    }

    public async ValueTask DisposeAsync() => await container.DisposeAsync();
}
