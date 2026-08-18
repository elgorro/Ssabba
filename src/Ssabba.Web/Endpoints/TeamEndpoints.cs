using Microsoft.EntityFrameworkCore;
using Ssabba.Domain;
using Ssabba.Domain.Entities;
using Ssabba.Infrastructure;
using Ssabba.Shared;

namespace Ssabba.Web.Endpoints;

/// <summary>Lineups: who plays together, and how each pairing has done.</summary>
public static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Teams).WithTags("Teams");

        group.MapGet("/", async (
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct,
            bool standingOnly = false) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await CommunityQueries.ResolveCommunityIdAsync(db, ct);

            return communityId is null
                ? Results.NotFound()
                : Results.Ok(await TeamQueries.ListAsync(db, communityId.Value, standingOnly, ct));
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await CommunityQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            var team = await TeamQueries.GetAsync(db, communityId.Value, id, ct);

            return team is null ? Results.NotFound() : Results.Ok(team);
        });

        group.MapGet("/{id:guid}/matches", async (
            Guid id,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await CommunityQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            return await TeamQueries.GetAsync(db, communityId.Value, id, ct) is null
                ? Results.NotFound()
                : Results.Ok(await TeamQueries.ListMatchesAsync(db, communityId.Value, id, ct));
        });

        // Forming a team is idempotent in the roster: an existing lineup comes back as 200 with its
        // id, so entering the same pair twice cannot leave two teams behind.
        group.MapPost("/", async (
            CreateTeamRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await CommunityQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            try
            {
                var (id, created) = await TeamQueries.GetOrCreateAsync(
                    db, communityId.Value, request.PlayerIds, request.Name, request.IsAdHoc, ct);

                return created ? Results.Created($"{ApiRoutes.Teams}/{id}", id) : Results.Ok(id);
            }
            catch (ArgumentException e)
            {
                // An unknown player or a lineup of one is the caller's mistake, not a fault.
                return Results.BadRequest(e.Message);
            }
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTeamRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await CommunityQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            try
            {
                return await TeamQueries.UpdateAsync(db, communityId.Value, id, request, ct)
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

            var communityId = await CommunityQueries.ResolveCommunityIdAsync(db, ct);
            if (communityId is null)
            {
                return Results.NotFound();
            }

            try
            {
                return await TeamQueries.DeleteAsync(db, communityId.Value, id, ct)
                    ? Results.NoContent()
                    : Results.NotFound();
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
        }).RequireAuthorization();

        return endpoints;
    }
}

/// <summary>
/// Shared query/command logic so server-rendered components and the API return identical results.
/// Every read filters by community by hand: there is no global query filter, by decision (ADR-0002).
/// </summary>
public static class TeamQueries
{
    public static async Task<List<TeamSummary>> ListAsync(
        SsabbaDbContext db,
        Guid communityId,
        bool standingOnly = false,
        CancellationToken ct = default)
    {
        // Display names are composed in memory: string.Join over the members has no SQL translation.
        var rows = await db.Teams
            .AsNoTracking()
            .Where(t => t.CommunityId == communityId)
            .Where(t => !standingOnly || !t.IsAdHoc)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.IsAdHoc,
                Members = t.Members.OrderBy(m => m.SortOrder).Select(m => m.Player!.DisplayName).ToList(),
                MatchesPlayed = db.Matches.Count(m =>
                    (m.HomeTeamId == t.Id || m.AwayTeamId == t.Id)
                    && m.Status == MatchStatus.Confirmed
                    && m.DeletedAt == null),
            })
            .ToListAsync(ct);

        return
        [
            .. rows
                .Select(r => new TeamSummary(
                    r.Id,
                    TeamNaming.Describe(r.Name, r.Members),
                    r.Name,
                    r.IsAdHoc,
                    r.Members,
                    r.MatchesPlayed))
                .OrderBy(t => t.DisplayName, StringComparer.CurrentCultureIgnoreCase),
        ];
    }

    public static async Task<TeamDetail?> GetAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid teamId,
        CancellationToken ct = default)
    {
        var row = await db.Teams
            .AsNoTracking()
            .Where(t => t.CommunityId == communityId && t.Id == teamId)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.IsAdHoc,
                Members = t.Members
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new
                    {
                        m.PlayerId,
                        m.Player!.DisplayName,
                        m.Player.Slug,
                        m.Position,
                        m.SortOrder,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            return null;
        }

        var (wins, losses) = await RecordAsync(db, communityId, teamId, ct);

        return new TeamDetail(
            row.Id,
            TeamNaming.Describe(row.Name, row.Members.Select(m => m.DisplayName)),
            row.Name,
            row.IsAdHoc,
            [.. row.Members.Select(m => new TeamMemberDto(
                m.PlayerId,
                m.DisplayName,
                m.Slug,
                m.Position.ToString(),
                m.SortOrder))],
            wins,
            losses,
            wins + losses);
    }

    /// <summary>
    /// The team's matches, each told from its side of the net. Only confirmed, undeleted matches
    /// count — a draft or a voided entry is not part of anyone's record.
    /// </summary>
    public static async Task<List<TeamMatchRow>> ListMatchesAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid teamId,
        CancellationToken ct = default)
    {
        var rows = await PlayedMatches(db, communityId, teamId)
            .OrderByDescending(m => m.PlayedAt)
            .Select(m => new
            {
                m.Id,
                m.PlayedAt,
                AtHome = m.HomeTeamId == teamId,
                HomeName = m.HomeTeam!.Name,
                // Both sides are fetched and the opponent picked in memory: ordering the members of
                // a navigation chosen by a conditional has no SQL translation.
                HomeMembers = m.HomeTeam.Members.OrderBy(x => x.SortOrder).Select(x => x.Player!.DisplayName).ToList(),
                AwayName = m.AwayTeam!.Name,
                AwayMembers = m.AwayTeam.Members.OrderBy(x => x.SortOrder).Select(x => x.Player!.DisplayName).ToList(),
                HomeSetsWon = m.Sets.Count(s => s.HomePoints > s.AwayPoints),
                AwaySetsWon = m.Sets.Count(s => s.AwayPoints > s.HomePoints),
            })
            .ToListAsync(ct);

        return
        [
            .. rows.Select(r =>
            {
                var setsFor = r.AtHome ? r.HomeSetsWon : r.AwaySetsWon;
                var setsAgainst = r.AtHome ? r.AwaySetsWon : r.HomeSetsWon;

                return new TeamMatchRow(
                    r.Id,
                    r.PlayedAt,
                    TeamNaming.Describe(
                        r.AtHome ? r.AwayName : r.HomeName,
                        r.AtHome ? r.AwayMembers : r.HomeMembers),
                    setsFor > setsAgainst,
                    setsFor,
                    setsAgainst);
            }),
        ];
    }

    /// <summary>
    /// Finds the team with this exact roster in the community, or forms it. The lineup is the key,
    /// so the order the players are given in only decides how the team is listed. When a roster that
    /// was thrown together for one match is submitted again with a name, it is promoted in place
    /// rather than copied.
    /// </summary>
    public static async Task<(Guid Id, bool Created)> GetOrCreateAsync(
        SsabbaDbContext db,
        Guid communityId,
        IReadOnlyList<Guid> playerIds,
        string? name,
        bool isAdHoc = true,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(playerIds);

        var ordered = playerIds.Distinct().ToList();

        if (ordered.Count < 2)
        {
            throw new ArgumentException("A team needs at least two players.", nameof(playerIds));
        }

        await EnsureMembersAsync(db, communityId, ordered, nameof(playerIds), ct);

        var memberKey = TeamRoster.Key(ordered);
        var trimmedName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

        var existing = await db.Teams
            .FirstOrDefaultAsync(t => t.CommunityId == communityId && t.MemberKey == memberKey, ct);

        if (existing is not null)
        {
            existing.Name = trimmedName ?? existing.Name;
            // Naming a lineup, or asking for it as a standing pairing, makes it one for good.
            existing.IsAdHoc = existing.IsAdHoc && isAdHoc;

            await db.SaveChangesAsync(ct);

            return (existing.Id, false);
        }

        var team = new Team
        {
            CommunityId = communityId,
            Name = trimmedName,
            IsAdHoc = isAdHoc,
            MemberKey = memberKey,
            Members = [.. ordered.Select((playerId, index) => new TeamMember
            {
                PlayerId = playerId,
                SortOrder = index,
            })],
        };

        db.Teams.Add(team);

        await db.SaveChangesAsync(ct);

        return (team.Id, true);
    }

    /// <summary>Returns <c>false</c> when no such team belongs to the community.</summary>
    public static async Task<bool> UpdateAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid teamId,
        UpdateTeamRequest request,
        CancellationToken ct = default)
    {
        var team = await db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.CommunityId == communityId && t.Id == teamId, ct);

        if (team is null)
        {
            return false;
        }

        var ordered = (request.PlayerIds ?? []).Distinct().ToList();

        if (ordered.Count < 2)
        {
            throw new ArgumentException("A team needs at least two players.", nameof(request));
        }

        await EnsureMembersAsync(db, communityId, ordered, nameof(request), ct);

        var memberKey = TeamRoster.Key(ordered);

        if (memberKey != team.MemberKey)
        {
            // Merging two teams' histories is not something an edit should do quietly, so a roster
            // that already belongs to another team is refused and the caller is told which.
            var clash = await db.Teams
                .AsNoTracking()
                .Where(t => t.CommunityId == communityId && t.MemberKey == memberKey && t.Id != teamId)
                .Select(t => new
                {
                    t.Name,
                    Members = t.Members.OrderBy(m => m.SortOrder).Select(m => m.Player!.DisplayName).ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (clash is not null)
            {
                throw new ArgumentException(
                    $"“{TeamNaming.Describe(clash.Name, clash.Members)}” already has these players.",
                    nameof(request));
            }
        }

        team.Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
        team.IsAdHoc = request.IsAdHoc;
        team.MemberKey = memberKey;

        db.TeamMembers.RemoveRange([.. team.Members.Where(m => !ordered.Contains(m.PlayerId))]);

        for (var index = 0; index < ordered.Count; index++)
        {
            var existing = team.Members.FirstOrDefault(m => m.PlayerId == ordered[index]);

            if (existing is null)
            {
                team.Members.Add(new TeamMember { TeamId = team.Id, PlayerId = ordered[index], SortOrder = index });
            }
            else
            {
                existing.SortOrder = index;
            }
        }

        await db.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// Removes a team that never played. One that did is kept: matches point at it, and deleting it
    /// would take the result with it.
    /// </summary>
    public static async Task<bool> DeleteAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid teamId,
        CancellationToken ct = default)
    {
        var team = await db.Teams
            .FirstOrDefaultAsync(t => t.CommunityId == communityId && t.Id == teamId, ct);

        if (team is null)
        {
            return false;
        }

        var matches = await db.Matches.CountAsync(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId, ct);

        if (matches > 0)
        {
            throw new ArgumentException(
                $"This team has played {matches} {(matches == 1 ? "match" : "matches")} and cannot be removed.",
                nameof(teamId));
        }

        db.Teams.Remove(team);

        await db.SaveChangesAsync(ct);

        return true;
    }

    private static async Task EnsureMembersAsync(
        SsabbaDbContext db,
        Guid communityId,
        List<Guid> playerIds,
        string parameterName,
        CancellationToken ct)
    {
        var known = await db.CommunityMembers
            .Where(m => m.CommunityId == communityId && m.Player!.DeletedAt == null)
            .Where(m => playerIds.Contains(m.PlayerId))
            .CountAsync(ct);

        if (known != playerIds.Count)
        {
            throw new ArgumentException("Every player must belong to this community.", parameterName);
        }
    }

    private static async Task<(int Wins, int Losses)> RecordAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid teamId,
        CancellationToken ct)
    {
        var rows = await PlayedMatches(db, communityId, teamId)
            .Select(m => new
            {
                AtHome = m.HomeTeamId == teamId,
                HomeSetsWon = m.Sets.Count(s => s.HomePoints > s.AwayPoints),
                AwaySetsWon = m.Sets.Count(s => s.AwayPoints > s.HomePoints),
            })
            .ToListAsync(ct);

        var wins = rows.Count(r => r.AtHome ? r.HomeSetsWon > r.AwaySetsWon : r.AwaySetsWon > r.HomeSetsWon);
        var losses = rows.Count(r => r.AtHome ? r.AwaySetsWon > r.HomeSetsWon : r.HomeSetsWon > r.AwaySetsWon);

        return (wins, losses);
    }

    private static IQueryable<Match> PlayedMatches(SsabbaDbContext db, Guid communityId, Guid teamId) =>
        db.Matches
            .AsNoTracking()
            .Where(m => m.CommunityId == communityId
                && (m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                && m.Status == MatchStatus.Confirmed
                && m.DeletedAt == null);
}
