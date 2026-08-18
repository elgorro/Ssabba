using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ssabba.Infrastructure;
using Ssabba.Web.Auth;

namespace Ssabba.Web.Tests;

/// <summary>
/// Stands in for the Keycloak handshake — the one edge that is genuinely external and cannot run in
/// a test. Everything else in these tests is the real thing. Requests opt in by sending
/// <see cref="UserHeader"/>; without it the request is anonymous.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IDbContextFactory<SsabbaDbContext> factory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";

    /// <summary>The subject a given test user signs in under, stable across requests.</summary>
    public static string SubjectFor(string user) => $"test|{user}";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers[UserHeader].FirstOrDefault() is not { Length: > 0 } user)
        {
            return AuthenticateResult.NoResult();
        }

        var identity = new ClaimsIdentity(
            [
                new Claim("sub", SubjectFor(user)),
                new Claim(ClaimTypes.NameIdentifier, user),
                new Claim("preferred_username", user),
            ],
            SchemeName,
            "preferred_username",
            "roles");

        var principal = new ClaimsPrincipal(identity);

        // This handler is where a test's sign-in happens, so it provisions the player exactly as the
        // OIDC handshake does — the same call, not a stand-in for it.
        await using (var db = await factory.CreateDbContextAsync(Context.RequestAborted))
        {
            await PlayerProvisioner.EnsureAsync(db, principal, Context.RequestAborted);
        }

        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }
}
