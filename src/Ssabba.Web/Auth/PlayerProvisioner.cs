using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Domain.Identity;
using Ssabba.Infrastructure;
using Ssabba.Web.Endpoints;

namespace Ssabba.Web.Auth;

/// <summary>
/// Turns an OIDC subject into a <see cref="Player"/> row, once, at sign-in.
/// </summary>
/// <remarks>
/// Signing in never adopts an existing account-less player, even when the names match exactly:
/// a player entered by hand carries a rating and a match history, and a new account must not
/// inherit either by accident. Claiming one is a deliberate flow of its own.
/// </remarks>
public static class PlayerProvisioner
{
    /// <summary>
    /// Returns the id of the player behind <paramref name="principal"/>, creating them on first
    /// sign-in. Idempotent: later sign-ins find the row by its subject and change nothing.
    /// </summary>
    public static async Task<Guid> EnsureAsync(
        SsabbaDbContext db,
        ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        var subjectId = principal.SubjectId()
            ?? throw new ArgumentException("The principal carries no \"sub\" claim.", nameof(principal));

        if (await FindBySubjectAsync(db, subjectId, ct) is { } existing)
        {
            return existing;
        }

        var username = principal.FindFirstValue("preferred_username");

        var player = new Player
        {
            DisplayName = principal.FindFirstValue("name")
                ?? username
                ?? subjectId,
            Slug = await ResolveSlugAsync(db, username, subjectId, ct),
            SubjectId = subjectId,
            Locale = Trim(principal.FindFirstValue("locale")),
        };

        db.Players.Add(player);

        // Membership needs a community to belong to. Creating the first one, and binding its owner,
        // is first-run work that happens elsewhere; until then a player simply belongs nowhere.
        if (await PlayerQueries.ResolveCommunityIdAsync(db, ct) is { } communityId)
        {
            db.CommunityMembers.Add(new CommunityMember
            {
                CommunityId = communityId,
                PlayerId = player.Id,
                Role = CommunityRole.Member,
            });
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two first sign-ins at once: the unique index on SubjectId settles it, and the loser
            // reads back whatever the winner wrote.
            db.ChangeTracker.Clear();

            if (await FindBySubjectAsync(db, subjectId, ct) is not { } winner)
            {
                throw;
            }

            return winner;
        }

        return player.Id;
    }

    private static async Task<Guid?> FindBySubjectAsync(SsabbaDbContext db, string subjectId, CancellationToken ct) =>
        await db.Players
            .AsNoTracking()
            .Where(p => p.SubjectId == subjectId && p.DeletedAt == null)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct);

    /// <summary>
    /// The name someone signs in under is theirs to choose and need not be unique, so a taken slug
    /// is numbered rather than refused — nobody may be locked out of their own first sign-in.
    /// </summary>
    private static async Task<string> ResolveSlugAsync(
        SsabbaDbContext db,
        string? username,
        string subjectId,
        CancellationToken ct)
    {
        var stem = PlayerSlug.From(username ?? string.Empty);

        if (stem.Length == 0)
        {
            stem = PlayerSlug.From($"player-{subjectId}");
        }

        if (stem.Length == 0)
        {
            stem = "player";
        }

        // Leave room for a numbering suffix within the column's length.
        stem = stem[..Math.Min(stem.Length, PlayerSlug.MaxLength - 4)];

        // Unique among the living only, matching the filtered index on the column.
        var taken = await db.Players
            .Where(p => p.DeletedAt == null && (p.Slug == stem || p.Slug.StartsWith(stem + "-")))
            .Select(p => p.Slug)
            .ToListAsync(ct);

        if (!taken.Contains(stem))
        {
            return stem;
        }

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"{stem}-{suffix}";

            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
