using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Domain.Rating;
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
                CourtName = m.Court != null ? m.Court.Venue!.Name + " \u2013 " + m.Court.Name : null,
                m.LocationNote,
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
            r.CourtName ?? r.LocationNote,
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

    /// <summary>
    /// Records a match and, because it is entered as agreed, immediately folds the result into the
    /// players' ratings. The community and format are derived from the teams rather than asked for:
    /// a team already belongs to a community, and its size is the format.
    /// </summary>
    public static async Task<Guid> CreateAsync(SsabbaDbContext db, CreateMatchRequest request, CancellationToken ct = default)
    {
        if (request.HomeTeamId == request.AwayTeamId)
        {
            throw new ArgumentException("A team cannot play against itself.", nameof(request));
        }

        var teams = await db.Teams
            .Where(t => t.Id == request.HomeTeamId || t.Id == request.AwayTeamId)
            .Select(t => new
            {
                t.Id,
                t.CommunityId,
                Members = t.Members.OrderBy(m => m.SortOrder).Select(m => m.PlayerId).ToList(),
            })
            .ToListAsync(ct);

        var home = teams.SingleOrDefault(t => t.Id == request.HomeTeamId)
            ?? throw new ArgumentException("Unknown home team.", nameof(request));
        var away = teams.SingleOrDefault(t => t.Id == request.AwayTeamId)
            ?? throw new ArgumentException("Unknown away team.", nameof(request));

        if (home.CommunityId != away.CommunityId)
        {
            throw new ArgumentException("Both teams must belong to the same community.", nameof(request));
        }

        var communityId = home.CommunityId;

        // The format is how many a side fielded. An uneven match is rated as the larger of the two.
        var playersPerSide = Math.Max(home.Members.Count, away.Members.Count);
        var format = await db.Formats.FirstOrDefaultAsync(f => f.PlayersPerSide == playersPerSide, ct)
            ?? throw new ArgumentException($"No format covers {playersPerSide} players a side.", nameof(request));

        var ruleSet = await db.RuleSets
            .FirstOrDefaultAsync(r => r.CommunityId == communityId && r.FormatId == format.Id && r.IsDefault, ct);

        var seasonId = await db.Seasons
            .Where(s => s.CommunityId == communityId && s.IsCurrent)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        var match = new Match
        {
            CommunityId = communityId,
            PlayedAt = request.PlayedAt,
            FormatId = format.Id,
            SeasonId = seasonId,
            CourtId = request.CourtId,
            LocationNote = request.LocationNote,
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            Status = MatchStatus.Confirmed,
            ConfirmedAt = DateTimeOffset.UtcNow,
            RuleSetId = ruleSet?.Id,
            // Snapshot the scoring, so later edits to the rule set cannot rewrite this result.
            SetsToWin = ruleSet?.SetsToWin ?? format.DefaultSetsToWin,
            PointsPerSet = ruleSet?.PointsPerSet ?? format.DefaultPointsPerSet,
            WinBy = ruleSet?.WinBy ?? format.DefaultWinBy,
            TiebreakPoints = ruleSet?.TiebreakPoints ?? format.DefaultTiebreakPoints,
            Sets = [.. request.Sets.Select(s => new MatchSet
            {
                Number = s.Number,
                HomePoints = s.HomePoints,
                AwayPoints = s.AwayPoints,
            })],
        };

        db.Matches.Add(match);

        await ApplyRatingAsync(db, match, communityId, format, home.Members, away.Members, ct);

        await db.SaveChangesAsync(ct);

        return match.Id;
    }

    /// <summary>
    /// Rates a confirmed match: moves each participant's community rating, writes the appearance
    /// rows that make the change explainable and replayable, and updates the per-format tallies.
    /// </summary>
    private static async Task ApplyRatingAsync(
        SsabbaDbContext db,
        Match match,
        Guid communityId,
        Format format,
        List<Guid> homePlayerIds,
        List<Guid> awayPlayerIds,
        CancellationToken ct)
    {
        if (homePlayerIds.Count == 0 || awayPlayerIds.Count == 0)
        {
            // Nobody to rate. The result still stands as a record.
            return;
        }

        var playerIds = homePlayerIds.Concat(awayPlayerIds).Distinct().ToList();

        var members = await db.CommunityMembers
            .Where(m => m.CommunityId == communityId && playerIds.Contains(m.PlayerId))
            .ToDictionaryAsync(m => m.PlayerId, ct);

        if (!playerIds.TrueForAll(members.ContainsKey))
        {
            throw new InvalidOperationException("Every player in a match must belong to its community.");
        }

        var homeRatings = homePlayerIds.Select(id => members[id].Rating).ToList();
        var awayRatings = awayPlayerIds.Select(id => members[id].Rating).ToList();

        var homeSetsWon = match.Sets.Count(s => s.HomePoints > s.AwayPoints);
        var awaySetsWon = match.Sets.Count(s => s.AwayPoints > s.HomePoints);

        var result = MatchRatingCalculator.Compute(
            homeRatings, awayRatings, homeSetsWon, awaySetsWon, format.RatingWeightPercent);

        Record(homePlayerIds, result.Home, MatchSide.Home, homeSetsWon, awaySetsWon);
        Record(awayPlayerIds, result.Away, MatchSide.Away, awaySetsWon, homeSetsWon);

        match.RatingAppliedAt = DateTimeOffset.UtcNow;

        void Record(
            List<Guid> sidePlayerIds,
            IReadOnlyList<MatchRatingCalculator.PlayerDelta> deltas,
            MatchSide side,
            int setsWon,
            int setsLost)
        {
            for (var i = 0; i < sidePlayerIds.Count; i++)
            {
                var member = members[sidePlayerIds[i]];
                var delta = deltas[i];

                match.Appearances.Add(new MatchAppearance
                {
                    PlayerId = member.PlayerId,
                    MemberId = member.Id,
                    Side = side,
                    RatingBefore = delta.Before,
                    RatingAfter = delta.After,
                    RatingDelta = delta.Delta,
                });

                member.Rating = delta.After;
                member.MatchesPlayed++;
            }
        }
    }
}

/// <summary>Turns a team into a display string.</summary>
public static class TeamNaming
{
    public static string Describe(string? teamName, IEnumerable<string> memberNames) =>
        string.IsNullOrWhiteSpace(teamName) ? string.Join(" / ", memberNames) : teamName;
}
