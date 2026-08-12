using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Infrastructure;
using Ssabba.Shared;

namespace Ssabba.Web.Endpoints;

/// <summary>Minimal API surface used by the WebAssembly client (the server renders from the DbContext directly).</summary>
public static class MatchEndpoints
{
    public static IEndpointRouteBuilder MapMatchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Matches).WithTags("Matches");

        group.MapGet("/", async (IDbContextFactory<SsabbaDbContext> factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return await MatchQueries.ListAsync(db, ct);
        });

        group.MapPost("/", async (
            CreateMatchRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var id = await MatchQueries.CreateAsync(db, request, ct);

            return Results.Created($"{ApiRoutes.Matches}/{id}", id);
        }).RequireAuthorization();

        endpoints.MapGet(ApiRoutes.Teams, async (IDbContextFactory<SsabbaDbContext> factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return await MatchQueries.ListTeamsAsync(db, ct);
        }).WithTags("Teams");

        return endpoints;
    }
}

/// <summary>
/// Shared query/command logic so server-rendered components and the API return identical results.
/// </summary>
public static class MatchQueries
{
    public static async Task<List<MatchSummary>> ListAsync(SsabbaDbContext db, CancellationToken ct = default)
    {
        // Team names are composed in memory: string.Join over the members has no SQL translation.
        var rows = await db.Matches
            .AsNoTracking()
            .OrderByDescending(m => m.PlayedAt)
            .Select(m => new
            {
                m.Id,
                m.PlayedAt,
                m.Location,
                HomeName = m.HomeTeam!.Name,
                HomeMembers = m.HomeTeam.Members.Select(x => x.Player!.DisplayName).ToList(),
                AwayName = m.AwayTeam!.Name,
                AwayMembers = m.AwayTeam.Members.Select(x => x.Player!.DisplayName).ToList(),
                HomeSetsWon = m.Sets.Count(s => s.HomePoints > s.AwayPoints),
                AwaySetsWon = m.Sets.Count(s => s.AwayPoints > s.HomePoints),
            })
            .ToListAsync(ct);

        return [.. rows.Select(r => new MatchSummary(
            r.Id,
            r.PlayedAt,
            r.Location,
            TeamNaming.Describe(r.HomeName, r.HomeMembers),
            TeamNaming.Describe(r.AwayName, r.AwayMembers),
            r.HomeSetsWon,
            r.AwaySetsWon))];
    }

    public static async Task<List<TeamOption>> ListTeamsAsync(SsabbaDbContext db, CancellationToken ct = default)
    {
        var rows = await db.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Name,
                Members = t.Members.Select(m => m.Player!.DisplayName).ToList(),
            })
            .ToListAsync(ct);

        return [.. rows.Select(r => new TeamOption(r.Id, TeamNaming.Describe(r.Name, r.Members)))];
    }

    public static async Task<Guid> CreateAsync(SsabbaDbContext db, CreateMatchRequest request, CancellationToken ct = default)
    {
        var match = new Match
        {
            PlayedAt = request.PlayedAt,
            Location = request.Location,
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            Sets = [.. request.Sets.Select(s => new MatchSet
            {
                Number = s.Number,
                HomePoints = s.HomePoints,
                AwayPoints = s.AwayPoints,
            })],
        };

        db.Matches.Add(match);
        await db.SaveChangesAsync(ct);

        return match.Id;
    }
}

/// <summary>Turns a team into a display string.</summary>
public static class TeamNaming
{
    public static string Describe(string? teamName, IEnumerable<string> memberNames) =>
        string.IsNullOrWhiteSpace(teamName) ? string.Join(" / ", memberNames) : teamName;
}
