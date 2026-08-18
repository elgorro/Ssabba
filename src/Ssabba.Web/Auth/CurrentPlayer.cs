using Microsoft.EntityFrameworkCore;
using Ssabba.Domain.Entities;
using Ssabba.Infrastructure;

namespace Ssabba.Web.Auth;

/// <summary>
/// Who the request is acting as: the player behind the signed-in subject, and the membership that
/// carries their standing. The membership is nullable — an instance with no community yet has
/// players who belong nowhere.
/// </summary>
public sealed record CurrentPlayer(
    Guid PlayerId,
    string DisplayName,
    string Slug,
    Guid? CommunityId,
    Guid? CommunityMemberId,
    CommunityRole? Role,
    MembershipStatus? Status)
{
    /// <summary>
    /// Standing enough to act here: a membership that has been accepted and not suspended. Role is
    /// a separate question — this only says the membership itself counts.
    /// </summary>
    public bool IsActiveMember => Status == MembershipStatus.Active && CommunityId is not null;
}

/// <summary>
/// Resolves <see cref="CurrentPlayer"/> for the request in hand, once. The row itself is written at
/// sign-in by <see cref="PlayerProvisioner"/>; this is only ever a lookup.
/// </summary>
/// <remarks>
/// Reads <c>HttpContext</c>, so this serves the minimal API and server-rendered components. An
/// interactive Blazor circuit has no live request and must be passed what it needs instead.
/// </remarks>
public sealed class CurrentPlayerAccessor(
    IHttpContextAccessor accessor,
    IDbContextFactory<SsabbaDbContext> factory)
{
    private CurrentPlayer? resolved;
    private bool attempted;

    /// <summary><c>null</c> when the request is anonymous, or carries a subject we hold no player for.</summary>
    public async Task<CurrentPlayer?> GetAsync(CancellationToken ct = default)
    {
        if (attempted)
        {
            return resolved;
        }

        attempted = true;

        if (accessor.HttpContext?.User is not { Identity.IsAuthenticated: true } user
            || user.SubjectId() is not { Length: > 0 } subjectId)
        {
            return null;
        }

        await using var db = await factory.CreateDbContextAsync(ct);

        var row = await db.Players
            .AsNoTracking()
            .Where(p => p.SubjectId == subjectId && p.DeletedAt == null)
            .Select(p => new
            {
                p.Id,
                p.DisplayName,
                p.Slug,
                // One instance, one community is the supported deployment; a member who has left is
                // not acting on anyone's behalf.
                Membership = p.Memberships
                    .Where(m => m.Status != MembershipStatus.Left)
                    .OrderBy(m => m.JoinedAt)
                    .Select(m => new { m.CommunityId, MemberId = m.Id, m.Role, m.Status })
                    .FirstOrDefault(),
            })
            .FirstOrDefaultAsync(ct);

        resolved = row is null
            ? null
            : new CurrentPlayer(
                row.Id,
                row.DisplayName,
                row.Slug,
                row.Membership?.CommunityId,
                row.Membership?.MemberId,
                row.Membership?.Role,
                row.Membership?.Status);

        return resolved;
    }
}
