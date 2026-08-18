using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Domain.Identity;
using Ssabba.Infrastructure;
using Ssabba.Shared;
using Ssabba.Web.Auth;

namespace Ssabba.Web.Endpoints;

/// <summary>
/// The community this instance is for: created once, on first run, by the person who then owns it.
/// </summary>
public static class CommunityEndpoints
{
    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Community).WithTags("Community");

        group.MapGet("/", async (
            IDbContextFactory<SsabbaDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var community = await CommunityQueries.GetAsync(db, ct);

            return community is null ? Results.NotFound() : Results.Ok(community);
        });

        group.MapPost("/", async (
            CreateCommunityRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CurrentPlayerAccessor current,
            CancellationToken ct) =>
        {
            // First run has no community, so the caller has no membership either: being a signed-in
            // player is all the standing there is to have at this point.
            if (await current.GetAsync(ct) is not { } player)
            {
                return Results.Forbid();
            }

            await using var db = await factory.CreateDbContextAsync(ct);

            return await CommunityQueries.CreateFirstAsync(db, player.PlayerId, request, ct) is { } created
                ? Results.Created(ApiRoutes.Community, created)
                : Results.Conflict(new { error = "This instance already has a community." });
        }).RequireAuthorization();

        group.MapPut("/", async (
            UpdateCommunityRequest request,
            IDbContextFactory<SsabbaDbContext> factory,
            CurrentPlayerAccessor current,
            CancellationToken ct) =>
        {
            // The first role check in the app. A policy-based scheme is its own piece of work; until
            // then the rule lives where it applies, in plain sight.
            if (await current.GetAsync(ct) is not { Role: CommunityRole.Owner or CommunityRole.Admin })
            {
                return Results.Forbid();
            }

            await using var db = await factory.CreateDbContextAsync(ct);

            return await CommunityQueries.UpdateAsync(db, request, ct)
                ? Results.NoContent()
                : Results.NotFound();
        }).RequireAuthorization();

        return endpoints;
    }
}

/// <summary>
/// Shared query/command logic so server-rendered components and the API return identical results.
/// </summary>
public static class CommunityQueries
{
    /// <summary>
    /// The community this instance is for. One instance, one community: several groups sharing an
    /// instance is not a supported state — federation (ADR-0002) is how communities meet — so a
    /// second row is a broken instance and throws rather than being guessed at.
    /// <c>null</c> until first run creates it.
    /// </summary>
    public static async Task<Guid?> ResolveCommunityIdAsync(SsabbaDbContext db, CancellationToken ct = default)
    {
        var ids = await db.Communities
            .AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .Take(2)
            .ToListAsync(ct);

        return ids.Count switch
        {
            0 => null,
            1 => ids[0],
            _ => throw new InvalidOperationException(
                "This instance holds more than one community, which it has no way to tell apart. "
                + "One instance, one community: see ADR-0002."),
        };
    }

    public static async Task<CommunityDetail?> GetAsync(SsabbaDbContext db, CancellationToken ct = default)
    {
        if (await ResolveCommunityIdAsync(db, ct) is not { } id)
        {
            return null;
        }

        return await db.Communities
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CommunityDetail(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.TimeZone,
                c.Currency,
                c.Visibility.ToString(),
                c.PublicKeyId,
                c.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Creates the community and binds <paramref name="playerId"/> to it as its owner. Returns
    /// <c>null</c> when the instance already has one — first run happens once.
    /// </summary>
    public static async Task<CommunityDetail?> CreateFirstAsync(
        SsabbaDbContext db,
        Guid playerId,
        CreateCommunityRequest request,
        CancellationToken ct = default)
    {
        if (await db.Communities.AnyAsync(ct))
        {
            return null;
        }

        var name = Require(request.Name);

        var community = new Community
        {
            Name = name,
            Slug = Slugify(request.Slug, name),
            Description = Trim(request.Description),
            TimeZone = Trim(request.TimeZone) ?? "UTC",
            Currency = NormalizeCurrency(request.Currency),
            Visibility = ParseVisibility(request.Visibility),
        };

        db.Communities.Add(community);

        // The one membership nobody grants: whoever sets the instance up owns what they set up.
        db.CommunityMembers.Add(new CommunityMember
        {
            CommunityId = community.Id,
            PlayerId = playerId,
            Role = CommunityRole.Owner,
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two people finishing the setup form at once: the unique slug index settles it, and the
            // loser is told what the winner already did.
            db.ChangeTracker.Clear();

            return null;
        }

        return new CommunityDetail(
            community.Id,
            community.Name,
            community.Slug,
            community.Description,
            community.TimeZone,
            community.Currency,
            community.Visibility.ToString(),
            community.PublicKeyId,
            community.CreatedAt);
    }

    /// <summary>
    /// Renames and reconfigures the community. <c>PublicKeyId</c> and <c>CreatedAt</c> are left
    /// alone: a link made to this community must survive whatever it calls itself now.
    /// Returns <c>false</c> when the instance has no community.
    /// </summary>
    public static async Task<bool> UpdateAsync(
        SsabbaDbContext db,
        UpdateCommunityRequest request,
        CancellationToken ct = default)
    {
        if (await ResolveCommunityIdAsync(db, ct) is not { } id
            || await db.Communities.FirstOrDefaultAsync(c => c.Id == id, ct) is not { } community)
        {
            return false;
        }

        community.Name = Require(request.Name);
        community.Slug = Slugify(request.Slug, community.Name);
        community.Description = Trim(request.Description);
        community.TimeZone = Trim(request.TimeZone) ?? "UTC";
        community.Currency = NormalizeCurrency(request.Currency);
        community.Visibility = ParseVisibility(request.Visibility);

        await db.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// The slug shape is the app's, not the player's, so a community is slugged by the same rules —
    /// same folding, same column length.
    /// </summary>
    private static string Slugify(string? requested, string name)
    {
        var slug = PlayerSlug.From(Trim(requested) ?? name);

        if (slug.Length == 0)
        {
            slug = PlayerSlug.From($"community-{Guid.CreateVersion7()}");
        }

        return slug[..Math.Min(slug.Length, PlayerSlug.MaxLength)];
    }

    private static CommunityVisibility ParseVisibility(string? value) =>
        Enum.TryParse<CommunityVisibility>(value, ignoreCase: true, out var parsed)
            ? parsed
            : CommunityVisibility.Private;

    /// <summary>The column is fixed-length; anything that is not a three-letter code is not one.</summary>
    private static string NormalizeCurrency(string? value)
    {
        var trimmed = Trim(value);

        return trimmed is { Length: 3 } ? trimmed.ToUpperInvariant() : "EUR";
    }

    private static string Require(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A community needs a name.", nameof(value))
            : value.Trim();

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
