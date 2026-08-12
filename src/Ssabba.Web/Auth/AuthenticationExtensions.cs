using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Ssabba.Web.Auth;

/// <summary>
/// OIDC wiring against Keycloak. The server acts as a BFF: tokens stay in the encrypted auth cookie and
/// never reach the WebAssembly client.
/// </summary>
public static class AuthenticationExtensions
{
    public const string LoginPath = "/authentication/login";
    public const string LogoutPath = "/authentication/logout";

    public static IServiceCollection AddSsabbaAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var oidc = configuration.GetSection("Oidc");

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "ssabba.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = true;
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = oidc["Authority"];

                // When the browser and the app reach Keycloak under different names (dev, or a
                // container network), fetch the discovery document over the internal address while
                // keeping the public issuer for validation and redirects.
                if (oidc["MetadataAddress"] is { Length: > 0 } metadataAddress)
                {
                    options.MetadataAddress = metadataAddress;
                }

                options.ClientId = oidc["ClientId"];
                options.ClientSecret = oidc["ClientSecret"];
                options.ResponseType = "code";
                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = oidc.GetValue("RequireHttpsMetadata", true);

                if (!options.RequireHttpsMetadata)
                {
                    // Local development over plain HTTP: the browser would drop these otherwise.
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.NonceCookie.SameSite = SameSiteMode.Lax;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                }

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                };
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        return services;
    }

    /// <summary>Maps the login/logout endpoints the UI links to.</summary>
    public static IEndpointRouteBuilder MapSsabbaAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(LoginPath, (string? returnUrl) =>
            TypedResults.Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" }));

        endpoints.MapPost(LogoutPath, () =>
            TypedResults.SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        return endpoints;
    }

    /// <summary>The stable identifier Ssabba stores on <c>Player.SubjectId</c>.</summary>
    public static string? SubjectId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue("sub");
}
