using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Domain.Identity;
using Ssabba.Infrastructure;
using Ssabba.Shared;

namespace Ssabba.Web.Endpoints;

/// <summary>The roster: who plays here, what they are called, and who has left.</summary>
public static class PlayerEndpoints
{
    public static IEndpointRouteBuilder MapPlayerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Players).WithTags("Players");

        group.MapGet("/", async (
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct,
            bool includeRetired = false) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var communityId = await PlayerQueries.ResolveCommunityIdAsync(db, ct);

            return communityId is null
                ? Results.NotFound()
                : Results.Ok(await PlayerQueries.ListAsync(db, communityId.Value, includeRetired, ct));
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

            var player = await PlayerQueries.GetAsync(db, communityId.Value, id, ct);

            return player is null ? Results.NotFound() : Results.Ok(player);
        });

        group.MapPost("/", async (
            CreatePlayerRequest request,
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
                var id = await PlayerQueries.CreateAsync(db, communityId.Value, request, ct);

                return Results.Created($"{ApiRoutes.Players}/{id}", id);
            }
            catch (ArgumentException e)
            {
                // A duplicate slug or an unknown role is the caller's mistake, not a fault.
                return Results.BadRequest(e.Message);
            }
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdatePlayerRequest request,
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
                return await PlayerQueries.UpdateAsync(db, communityId.Value, id, request, ct)
                    ? Results.NoContent()
                    : Results.NotFound();
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/retire", async (
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

            return await PlayerQueries.SetRetiredAsync(db, communityId.Value, id, retired: true, ct)
                ? Results.NoContent()
                : Results.NotFound();
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/reinstate", async (
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

            return await PlayerQueries.SetRetiredAsync(db, communityId.Value, id, retired: false, ct)
                ? Results.NoContent()
                : Results.NotFound();
        }).RequireAuthorization();

        return endpoints;
    }
}

/// <summary>
/// Shared query/command logic so server-rendered components and the API return identical results.
/// Every read filters by community and skips erased players by hand: there is no global query
/// filter, by decision (ADR-0002).
/// </summary>
public static class PlayerQueries
{
    /// <summary>
    /// The community this instance is for. One instance, one community is the supported deployment,
    /// so the roster does not ask which one; <c>null</c> means the instance has none yet.
    /// </summary>
    public static async Task<Guid?> ResolveCommunityIdAsync(SsabbaDbContext db, CancellationToken ct = default) =>
        await db.Communities
            .AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

    public static async Task<List<PlayerSummary>> ListAsync(
        SsabbaDbContext db,
        Guid communityId,
        bool includeRetired = false,
        CancellationToken ct = default)
    {
        var rows = await db.CommunityMembers
            .AsNoTracking()
            .Where(m => m.CommunityId == communityId && m.Player!.DeletedAt == null)
            .Where(m => includeRetired || m.Status != MembershipStatus.Left)
            .OrderBy(m => m.Player!.DisplayName)
            .Select(m => new
            {
                m.PlayerId,
                m.Player!.DisplayName,
                m.Player.Slug,
                m.Nickname,
                m.Role,
                m.Status,
                m.Rating,
                m.MatchesPlayed,
            })
            .ToListAsync(ct);

        return [.. rows.Select(r => new PlayerSummary(
            r.PlayerId,
            r.DisplayName,
            r.Slug,
            r.Nickname,
            r.Role.ToString(),
            r.Status == MembershipStatus.Left,
            r.Rating,
            r.MatchesPlayed))];
    }

    public static Task<PlayerDetail?> GetAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid playerId,
        CancellationToken ct = default) =>
        FindAsync(db, communityId, m => m.PlayerId == playerId, ct);

    public static Task<PlayerDetail?> GetBySlugAsync(
        SsabbaDbContext db,
        Guid communityId,
        string slug,
        CancellationToken ct = default) =>
        FindAsync(db, communityId, m => m.Player!.Slug == slug, ct);

    private static async Task<PlayerDetail?> FindAsync(
        SsabbaDbContext db,
        Guid communityId,
        System.Linq.Expressions.Expression<Func<CommunityMember, bool>> predicate,
        CancellationToken ct)
    {
        var row = await db.CommunityMembers
            .AsNoTracking()
            .Where(m => m.CommunityId == communityId && m.Player!.DeletedAt == null)
            .Where(predicate)
            .Select(m => new
            {
                m.PlayerId,
                m.Player!.DisplayName,
                m.Player.Slug,
                m.Player.PreferredTimeZone,
                m.Player.Locale,
                m.Player.SubjectId,
                m.Nickname,
                m.Role,
                m.Status,
                m.Rating,
                m.MatchesPlayed,
                m.JoinedAt,
                m.LeftAt,
                Profile = m.Player.Profile,
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            return null;
        }

        return new PlayerDetail(
            row.PlayerId,
            row.DisplayName,
            row.Slug,
            row.PreferredTimeZone,
            row.Locale,
            row.SubjectId is not null,
            row.Nickname,
            row.Role.ToString(),
            row.Status == MembershipStatus.Left,
            row.Rating,
            row.MatchesPlayed,
            row.JoinedAt,
            row.LeftAt,
            Describe(row.Profile));
    }

    /// <summary>
    /// Adds a player to the community: the instance-wide identity and the membership that carries
    /// their standing here. The player has no account until they sign in themselves.
    /// </summary>
    public static async Task<Guid> CreateAsync(
        SsabbaDbContext db,
        Guid communityId,
        CreatePlayerRequest request,
        CancellationToken ct = default)
    {
        var displayName = Require(request.DisplayName);
        var slug = await ResolveSlugAsync(db, request.Slug, displayName, existingPlayerId: null, ct);

        var player = new Player
        {
            DisplayName = displayName,
            Slug = slug,
            PreferredTimeZone = Trim(request.PreferredTimeZone),
            Locale = Trim(request.Locale),
        };

        Apply(player, request.Profile);

        db.Players.Add(player);

        db.CommunityMembers.Add(new CommunityMember
        {
            CommunityId = communityId,
            PlayerId = player.Id,
            Nickname = Trim(request.Nickname),
            Role = ParseRole(request.Role),
        });

        await db.SaveChangesAsync(ct);

        return player.Id;
    }

    /// <summary>Returns <c>false</c> when no such player belongs to the community.</summary>
    public static async Task<bool> UpdateAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid playerId,
        UpdatePlayerRequest request,
        CancellationToken ct = default)
    {
        var membership = await db.CommunityMembers
            .Include(m => m.Player)
            .ThenInclude(p => p!.Profile)
            .FirstOrDefaultAsync(
                m => m.CommunityId == communityId && m.PlayerId == playerId && m.Player!.DeletedAt == null,
                ct);

        if (membership?.Player is not { } player)
        {
            return false;
        }

        player.DisplayName = Require(request.DisplayName);
        player.Slug = await ResolveSlugAsync(db, request.Slug, player.DisplayName, player.Id, ct);
        player.PreferredTimeZone = Trim(request.PreferredTimeZone);
        player.Locale = Trim(request.Locale);

        Apply(player, request.Profile);

        membership.Nickname = Trim(request.Nickname);
        membership.Role = ParseRole(request.Role);

        await db.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// Retires a player from the community, or brings them back. Only the membership status moves:
    /// the rating, the tally and every match appearance stay exactly as they were.
    /// </summary>
    public static async Task<bool> SetRetiredAsync(
        SsabbaDbContext db,
        Guid communityId,
        Guid playerId,
        bool retired,
        CancellationToken ct = default)
    {
        var membership = await db.CommunityMembers
            .FirstOrDefaultAsync(
                m => m.CommunityId == communityId && m.PlayerId == playerId && m.Player!.DeletedAt == null,
                ct);

        if (membership is null)
        {
            return false;
        }

        membership.Status = retired ? MembershipStatus.Left : MembershipStatus.Active;
        membership.LeftAt = retired ? DateTimeOffset.UtcNow : null;

        await db.SaveChangesAsync(ct);

        return true;
    }

    private static async Task<string> ResolveSlugAsync(
        SsabbaDbContext db,
        string? requested,
        string displayName,
        Guid? existingPlayerId,
        CancellationToken ct)
    {
        var slug = PlayerSlug.From(string.IsNullOrWhiteSpace(requested) ? displayName : requested);

        if (slug.Length == 0)
        {
            throw new ArgumentException("The display name yields no usable slug; give one explicitly.", nameof(requested));
        }

        // Unique among the living only, matching the filtered index on the column.
        var taken = await db.Players
            .AnyAsync(p => p.Slug == slug && p.DeletedAt == null && p.Id != existingPlayerId, ct);

        return taken
            ? throw new ArgumentException($"The slug \"{slug}\" is already taken.", nameof(requested))
            : slug;
    }

    private static void Apply(Player player, PlayerProfileDto? profile)
    {
        if (profile is null)
        {
            return;
        }

        player.Profile ??= new PlayerProfile();

        player.Profile.HeightCm = profile.HeightCm;
        player.Profile.PreferredPositions = ParsePositions(profile.PreferredPositions);
        player.Profile.SelfRatedLevel = profile.SelfRatedLevel;
        player.Profile.PlayingSince = profile.PlayingSince;
        player.Profile.Bio = Trim(profile.Bio);
        player.Profile.IsLeftHanded = profile.IsLeftHanded;
    }

    private static PlayerProfileDto Describe(PlayerProfile? profile) =>
        profile is null
            ? PlayerProfileDto.Empty
            : new PlayerProfileDto(
                profile.HeightCm,
                [.. Enum.GetValues<PlayingPosition>()
                    .Where(p => p != PlayingPosition.None && profile.PreferredPositions.HasFlag(p))
                    .Select(p => p.ToString())],
                profile.SelfRatedLevel,
                profile.PlayingSince,
                profile.Bio,
                profile.IsLeftHanded);

    private static PlayingPosition ParsePositions(IReadOnlyList<string>? names)
    {
        var positions = PlayingPosition.None;

        foreach (var name in names ?? [])
        {
            if (!Enum.TryParse<PlayingPosition>(name, ignoreCase: true, out var position))
            {
                throw new ArgumentException($"Unknown playing position \"{name}\".", nameof(names));
            }

            positions |= position;
        }

        return positions;
    }

    private static CommunityRole ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return CommunityRole.Member;
        }

        return Enum.TryParse<CommunityRole>(role, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Unknown community role \"{role}\".", nameof(role));
    }

    private static string Require(string? displayName) =>
        string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("A player needs a display name.", nameof(displayName))
            : displayName.Trim();

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
