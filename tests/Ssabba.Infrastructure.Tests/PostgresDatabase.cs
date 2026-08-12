using Ssabba.TestSupport;

namespace Ssabba.Infrastructure.Tests;

/// <summary>
/// One PostgreSQL container for every integration test in this assembly. xUnit requires the
/// definition to live in the assembly it applies to, so each test project declares its own.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresDatabase : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
