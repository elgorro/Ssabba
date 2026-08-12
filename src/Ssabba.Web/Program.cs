using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Ssabba.Infrastructure;
using Ssabba.Web.Auth;
using Ssabba.Web.Components;
using Ssabba.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Structured JSON on stdout; Promtail ships it to Loki for Grafana.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    // Flows the signed-in user to the WebAssembly client without exposing tokens.
    .AddAuthenticationStateSerialization();

builder.Services.AddSsabbaInfrastructure(
    builder.Configuration.GetConnectionString("Ssabba")
    ?? throw new InvalidOperationException("Connection string 'Ssabba' is not configured."));

builder.Services.AddSsabbaAuthentication(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<SsabbaDbContext>("database");

// Keep auth cookies valid across container restarts and replicas.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["DataProtection:KeyPath"] ?? "/keys"))
    .SetApplicationName("ssabba");

// Behind Traefik: honour the forwarded scheme/host so OIDC redirect URIs are built correctly.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Only for the UI: re-executing keeps the request method, so an API 401 would come back as the
// Blazor page's 400 and the client would never see the real status.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapSsabbaAuthEndpoints();
app.MapMatchEndpoints();
app.MapHealthChecks("/healthz").AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Ssabba.Web.Client._Imports).Assembly);

// Applying migrations on start keeps self-hosted upgrades to a single `docker compose up`.
if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SsabbaDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

app.Run();

/// <summary>Exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can boot the real host in tests.</summary>
public partial class Program;
