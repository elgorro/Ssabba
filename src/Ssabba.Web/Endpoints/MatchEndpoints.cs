using Microsoft.EntityFrameworkCore;
using Ssabba.Domain;
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

        group.MapGet("/", async (
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct,
            Guid? playerId = null,
            Guid? teamId = null,
            DateOnly? from = null,
            DateOnly? to = null,
            int page = 1,
            int pageSize = 25) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await PlayerQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return new PagedResult<MatchSummary>([], page, pageSize, 0);
            }

            return await MatchQueries.ListAsync(
                db,
                communityId.Value,
                new MatchFilter(playerId, teamId, from, to, page, pageSize),
                ct);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await PlayerQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            var match = await MatchQueries.GetAsync(db, communityId.Value, id, ct);

            return match is null ? Results.NotFound() : Results.Ok(match);
        });

        group.MapPost("/", async (
            CreateMatchRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            try
            {
                var id = await MatchQueries.CreateAsync(db, request, ct);

                return Results.Created($"{ApiRoutes.Matches}/{id}", id);
            }
            catch (ArgumentException e)
            {
                // An unknown team or an impossible score is the caller's mistake, not a fault.
                return Results.BadRequest(e.Message);
            }
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMatchRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await PlayerQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            try
            {
                return await MatchQueries.UpdateAsync(db, communityId.Value, id, request, ct)
                    ? Results.NoContent()
                    : Results.NotFound();
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await PlayerQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            return await MatchQueries.DeleteAsync(db, communityId.Value, id, ct)
                ? Results.NoContent()
                : Results.NotFound();
        }).RequireAuthorization();

        return endpoints;
    }
}

/// <summary>
/// Shared query/command logic so server-rendered components and the API return identical results.
/// </summary>
public static class MatchQueries
{
    /// <summary>Largest page a caller may ask for: a filter typo should not read the whole table.</summary>
    private const int MaxPageSize = 100;

    public static async Task<PagedResult<MatchSummary>> ListAsync(
        SsabbaDbContext db,
        Guid communityId,
        MatchFilter filter,
        CancellationToken ct = default)
    {
        // Clamped here rather than at the edge, so the page and the API agree on what page 0 means.
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

        var query = db.Matches
            .AsNoTracking()
            .Where(m => m.CommunityId == communityId && m.DeletedAt == null);

        if (filter.TeamId is { } teamId)
        {
            query = query.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);
        }

        if (filter.PlayerId is { } playerId)
        {
            // The lineup, not the appearances: a match should list whether or not it has been rated.
            query = query.Where(m =>
                m.HomeTeam!.Members.Any(x => x.PlayerId == playerId) ||
                m.AwayTeam!.Members.Any(x => x.PlayerId == playerId));
        }

        if (filter.From is { } from)
        {
            var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(m => m.PlayedAt >= start);
        }

        if (filter.To is { } to)
        {
            // The whole of the closing day counts, hence the next midnight rather than that one.
            var end = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(m => m.PlayedAt < end);
        }

        var total = await query.CountAsync(ct);

        // Team names are composed in memory: string.Join over the members has no SQL translation.
        var rows = await query
            .OrderByDescending(m => m.PlayedAt)
            // Two matches can share an instant; without a tiebreak the pages overlap.
            .ThenBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        List<MatchSummary> items = [.. rows.Select(r => new MatchSummary(
            r.Id,
            r.PlayedAt,
            r.CourtName ?? r.LocationNote,
            TeamNaming.Describe(r.HomeName, r.HomeMembers),
            TeamNaming.Describe(r.AwayName, r.AwayMembers),
            r.HomeSetsWon,
            r.AwaySetsWon))];

        return new PagedResult<MatchSummary>(items, page, pageSize, total);
    }

    /// <summary>One match with its sets, or <c>null</c> when the community has no such match.</summary>
    public static async Task<MatchDetail?> GetAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid matchId,
        CancellationToken ct = default)
    {
        // Team names are composed in memory: string.Join over the members has no SQL translation.
        var row = await db.Matches
            .AsNoTracking()
            .Where(m => m.CommunityId == communityId && m.Id == matchId && m.DeletedAt == null)
            .Select(m => new
            {
                m.Id,
                m.PlayedAt,
                CourtName = m.Court != null ? m.Court.Venue!.Name + " \u2013 " + m.Court.Name : null,
                m.LocationNote,
                m.HomeTeamId,
                HomeName = m.HomeTeam!.Name,
                HomeMembers = m.HomeTeam.Members.OrderBy(x => x.SortOrder).Select(x => x.Player!.DisplayName).ToList(),
                m.AwayTeamId,
                AwayName = m.AwayTeam!.Name,
                AwayMembers = m.AwayTeam.Members.OrderBy(x => x.SortOrder).Select(x => x.Player!.DisplayName).ToList(),
                Sets = m.Sets.OrderBy(s => s.Number).Select(s => new SetScore(s.Number, s.HomePoints, s.AwayPoints)).ToList(),
                m.Status,
                m.SetsToWin,
                m.PointsPerSet,
                m.WinBy,
                m.TiebreakPoints,
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            return null;
        }

        var homeSetsWon = row.Sets.Count(s => s.HomePoints > s.AwayPoints);
        var awaySetsWon = row.Sets.Count(s => s.AwayPoints > s.HomePoints);

        // The winner follows from the sets. There is no field for it and never should be.
        var outcome = homeSetsWon.CompareTo(awaySetsWon) switch
        {
            > 0 => MatchOutcome.HomeWin,
            < 0 => MatchOutcome.AwayWin,
            _ => MatchOutcome.Undecided,
        };

        return new MatchDetail(
            row.Id,
            row.PlayedAt,
            row.CourtName ?? row.LocationNote,
            row.LocationNote,
            row.HomeTeamId,
            TeamNaming.Describe(row.HomeName, row.HomeMembers),
            row.AwayTeamId,
            TeamNaming.Describe(row.AwayName, row.AwayMembers),
            row.Sets,
            homeSetsWon,
            awaySetsWon,
            outcome.ToString(),
            row.Status.ToString(),
            row.SetsToWin,
            row.PointsPerSet,
            row.WinBy,
            row.TiebreakPoints);
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

        var setsToWin = ruleSet?.SetsToWin ?? format.DefaultSetsToWin;
        var pointsPerSet = ruleSet?.PointsPerSet ?? format.DefaultPointsPerSet;
        var winBy = ruleSet?.WinBy ?? format.DefaultWinBy;
        var tiebreakPoints = ruleSet?.TiebreakPoints ?? format.DefaultTiebreakPoints;

        // Checked against the rules this match is being played under, not against the official ones.
        Ensure(request.Sets, setsToWin, pointsPerSet, winBy, tiebreakPoints, nameof(request));

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
            SetsToWin = setsToWin,
            PointsPerSet = pointsPerSet,
            WinBy = winBy,
            TiebreakPoints = tiebreakPoints,
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

                var appearance = new MatchAppearance
                {
                    PlayerId = member.PlayerId,
                    MemberId = member.Id,
                    Side = side,
                    RatingBefore = delta.Before,
                    RatingAfter = delta.After,
                    RatingDelta = delta.Delta,
                };

                match.Appearances.Add(appearance);

                // Stated as new rather than inferred. The key is generated here rather than by the
                // database, so EF hangs a child off an already-tracked match as a row to update.
                db.MatchAppearances.Add(appearance);

                member.Rating = delta.After;
                member.MatchesPlayed++;
            }
        }
    }

    /// <summary>
    /// Corrects a recorded match. The scores are checked against the match's own snapshot rather
    /// than against the current rule set: a match keeps the rules it was played under, and fixing a
    /// typo must not quietly re-rate it under rules nobody played.
    /// </summary>
    /// <remarks>
    /// The rating is reversed and applied again, so a corrected score leaves the ladder where
    /// recording it correctly would have. Teams are deliberately not editable here — changing who
    /// played is a different match, and the ratings of two other people would have to move.
    /// </remarks>
    /// <returns><c>false</c> when the community has no such match.</returns>
    public static async Task<bool> UpdateAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid matchId,
        UpdateMatchRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = await db.Matches
            .Include(m => m.Sets)
            .Include(m => m.Appearances)
            .FirstOrDefaultAsync(
                m => m.CommunityId == communityId && m.Id == matchId && m.DeletedAt == null, ct);

        if (match is null)
        {
            return false;
        }

        // Who played is not editable here: a different pairing is a different match, and two other
        // people's ratings would have to move with it. Say so rather than ignoring the fields.
        if (request.HomeTeamId != match.HomeTeamId || request.AwayTeamId != match.AwayTeamId)
        {
            throw new ArgumentException(
                "The teams in a recorded match cannot be changed. Record the right match instead.",
                nameof(request));
        }

        Ensure(request.Sets, match.SetsToWin, match.PointsPerSet, match.WinBy, match.TiebreakPoints, nameof(request));

        // Taking the old rows away and putting the new ones back has to be two saves: the unique
        // indexes on (MatchId, Number) and (MatchId, PlayerId) are checked per statement, and EF is
        // free to order the inserts before the deletes. One transaction keeps that invisible.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        await ReverseRatingAsync(db, match, communityId, ct);

        db.MatchSets.RemoveRange(match.Sets);

        await db.SaveChangesAsync(ct);

        // Start the second half from a clean tracker. The spent sets and appearances are gone from
        // the database but still known to this context, and EF's fixup would pull them back onto the
        // match as rows to update the moment new ones are added beside them.
        db.ChangeTracker.Clear();

        match = await db.Matches.FirstAsync(m => m.Id == matchId, ct);

        match.PlayedAt = request.PlayedAt;
        match.CourtId = request.CourtId;
        match.LocationNote = request.LocationNote;

        foreach (var set in request.Sets.OrderBy(s => s.Number))
        {
            var replacement = new MatchSet
            {
                Number = set.Number,
                HomePoints = set.HomePoints,
                AwayPoints = set.AwayPoints,
            };

            match.Sets.Add(replacement);
            db.MatchSets.Add(replacement);
        }

        var format = await db.Formats.FirstAsync(f => f.Id == match.FormatId, ct);
        var (home, away) = await SidesAsync(db, match, ct);

        await ApplyRatingAsync(db, match, communityId, format, home, away, ct);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }

    /// <summary>
    /// Strikes a match from the record and gives back the rating it took. The row stays: the
    /// appearances are the ladder's history, and a result that vanishes cannot explain a rating that
    /// has already moved. Every query filters on <c>DeletedAt</c> instead.
    /// </summary>
    /// <returns><c>false</c> when the community has no such match.</returns>
    public static async Task<bool> DeleteAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid matchId,
        CancellationToken ct = default)
    {
        var match = await db.Matches
            .Include(m => m.Appearances)
            .FirstOrDefaultAsync(
                m => m.CommunityId == communityId && m.Id == matchId && m.DeletedAt == null, ct);

        if (match is null)
        {
            return false;
        }

        await ReverseRatingAsync(db, match, communityId, ct);

        match.Status = MatchStatus.Voided;
        match.DeletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// Takes back what this match did to the ratings, using the deltas stored on its appearances —
    /// which is why they are written down rather than recomputed.
    /// </summary>
    /// <remarks>
    /// Exact only while nobody involved has played since: Elo is path-dependent, so a later match
    /// rated against a rating this one set would need re-rating too. Replaying the whole history is
    /// tracked as issue #24, and this method together with <c>ApplyRatingAsync</c> is what it will
    /// build on. Neither saves; the caller owns the transaction.
    /// </remarks>
    private static async Task ReverseRatingAsync(
        SsabbaDbContext db,
        Match match,
        Guid communityId,
        CancellationToken ct)
    {
        if (match.RatingAppliedAt is null || match.Appearances.Count == 0)
        {
            return;
        }

        var playerIds = match.Appearances.Select(a => a.PlayerId).ToList();

        var members = await db.CommunityMembers
            .Where(m => m.CommunityId == communityId && playerIds.Contains(m.PlayerId))
            .ToDictionaryAsync(m => m.PlayerId, ct);

        foreach (var appearance in match.Appearances)
        {
            if (members.TryGetValue(appearance.PlayerId, out var member))
            {
                member.Rating -= appearance.RatingDelta;
                member.MatchesPlayed--;
            }
        }

        // Left on the navigation on purpose: taking a tracked child off its parent has EF treat it
        // as an orphan to update rather than as a row to remove.
        db.MatchAppearances.RemoveRange(match.Appearances);
        match.RatingAppliedAt = null;
    }

    /// <summary>The player ids of each side, in the order the lineups list them.</summary>
    private static async Task<(List<Guid> Home, List<Guid> Away)> SidesAsync(
        SsabbaDbContext db,
        Match match,
        CancellationToken ct)
    {
        var teams = await db.Teams
            .AsNoTracking()
            .Where(t => t.Id == match.HomeTeamId || t.Id == match.AwayTeamId)
            .Select(t => new
            {
                t.Id,
                Members = t.Members.OrderBy(m => m.SortOrder).Select(m => m.PlayerId).ToList(),
            })
            .ToListAsync(ct);

        return (
            teams.SingleOrDefault(t => t.Id == match.HomeTeamId)?.Members ?? [],
            teams.SingleOrDefault(t => t.Id == match.AwayTeamId)?.Members ?? []);
    }

    /// <summary>
    /// Refuses a score that could not have been played under these rules, in the words the API hands
    /// straight back to whoever typed it.
    /// </summary>
    private static void Ensure(
        IReadOnlyList<SetScore> sets,
        int setsToWin,
        int pointsPerSet,
        int winBy,
        int tiebreakPoints,
        string parameterName)
    {
        var scores = (sets ?? [])
            .Select(s => new MatchScoring.Set(s.Number, s.HomePoints, s.AwayPoints))
            .ToList();

        if (MatchScoring.Validate(scores, setsToWin, pointsPerSet, winBy, tiebreakPoints) is { } complaint)
        {
            throw new ArgumentException(complaint, parameterName);
        }

        if (!MatchScoring.IsDecided(scores, setsToWin))
        {
            throw new ArgumentException(
                $"Nobody has won {setsToWin} set{(setsToWin == 1 ? string.Empty : "s")} yet, "
                + "so the match is not finished.",
                parameterName);
        }
    }
}

/// <summary>Turns a team into a display string.</summary>
public static class TeamNaming
{
    public static string Describe(string? teamName, IEnumerable<string> memberNames) =>
        string.IsNullOrWhiteSpace(teamName) ? string.Join(" / ", memberNames) : teamName;
}
