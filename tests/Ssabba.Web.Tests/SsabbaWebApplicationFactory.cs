using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ssabba.TestSupport;

namespace Ssabba.Web.Tests;

/// <summary>
/// The real host, with three substitutions: the database points at the test container, the OIDC
/// handshake is replaced by <see cref="TestAuthHandler"/>, and the clock is one a test can move.
/// Migrations are applied by the fixture, so startup migration is switched off.
/// </summary>
public sealed class SsabbaWebApplicationFactory(PostgresFixture postgres) : WebApplicationFactory<Program>
{
    private readonly string keyPath = Directory.CreateTempSubdirectory("ssabba-keys").FullName;

    /// <summary>The host's clock, for tests about how long something stays amendable.</summary>
    public TestClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.UseSetting("ConnectionStrings:Ssabba", postgres.ConnectionString);
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("DataProtection:KeyPath", keyPath);
        builder.UseSetting("Oidc:Authority", "https://keycloak.invalid/realms/ssabba");
        builder.UseSetting("Oidc:ClientId", "ssabba-tests");
        builder.UseSetting("Oidc:ClientSecret", "not-a-secret");

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(Clock);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    /// <summary>A client that is signed in as <paramref name="user"/> for every request.</summary>
    public HttpClient CreateClientAs(string user)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, user);

        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(keyPath))
        {
            Directory.Delete(keyPath, recursive: true);
        }
    }
}
