using Microsoft.AspNetCore.Authorization;
using Ssabba.Domain.Entities;

namespace Ssabba.Web.Auth;

/// <summary>
/// What a request is allowed to do here, expressed as policies rather than as checks scattered
/// through the endpoints.
/// </summary>
/// <remarks>
/// These read <see cref="CurrentPlayerAccessor"/>, not the <c>roles</c> claim. Keycloak's realm roles
/// say what someone is on this instance; the authority for what they may do is
/// <see cref="CommunityMember.Role"/>, because — as the entity puts it — permissions are relative to
/// a community, never global.
/// </remarks>
public static class CommunityPolicies
{
    /// <summary>An accepted, unsuspended membership. Enough to record what you played.</summary>
    public const string ActiveMember = "community:active-member";

    /// <summary>Admin and above: may also configure the community itself.</summary>
    public const string Administrator = "community:administrator";

    /// <summary>
    /// May correct or strike one particular result. Resource-based: authorize against a
    /// <see cref="MatchAmendContext"/> rather than by calling <c>RequireAuthorization</c> alone.
    /// </summary>
    public const string AmendMatch = "match:amend";

    /// <summary>
    /// How long a participant may go on correcting their own result when nothing overrides it. Long
    /// enough that a Friday evening's play is still fixable on Monday morning, and short enough that
    /// the ladder settles: reversing a rating is only exact while nobody involved has played since.
    /// </summary>
    public static readonly TimeSpan DefaultAmendWindow = TimeSpan.FromHours(60);

    public static AuthorizationOptions AddSsabbaPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(ActiveMember, policy =>
            policy.AddRequirements(new CommunityRoleRequirement(CommunityRole.Member)));

        options.AddPolicy(Administrator, policy =>
            policy.AddRequirements(new CommunityRoleRequirement(CommunityRole.Admin)));

        options.AddPolicy(AmendMatch, policy =>
            policy.AddRequirements(new AmendMatchRequirement()));

        return options;
    }
}

/// <summary>Standing in the community, at or above <paramref name="Minimum"/>.</summary>
/// <remarks>
/// <see cref="CommunityRole"/> is ordered from Guest to Owner precisely so this can be a comparison
/// rather than a list of names to keep in step.
/// </remarks>
public sealed record CommunityRoleRequirement(CommunityRole Minimum) : IAuthorizationRequirement;

public sealed class CommunityRoleHandler(CurrentPlayerAccessor current)
    : AuthorizationHandler<CommunityRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CommunityRoleRequirement requirement)
    {
        if (await current.GetAsync() is { IsActiveMember: true, Role: { } role } && role >= requirement.Minimum)
        {
            context.Succeed(requirement);
        }
    }
}

/// <summary>
/// What has to be known about a match before anyone can be told whether they may amend it. Assembled
/// by <c>MatchQueries.AmendContextAsync</c>, which is the only thing that builds one.
/// </summary>
/// <param name="RecordedAt">
/// When the result was entered, not when it was played — the window is about how long ago somebody
/// typed it. An edit does not reset it, or a match could be kept amendable forever.
/// </param>
/// <param name="ParticipantPlayerIds">The two lineups, which is who has a claim on this result.</param>
public sealed record MatchAmendContext(
    Guid CommunityId,
    DateTimeOffset RecordedAt,
    IReadOnlyList<Guid> ParticipantPlayerIds,
    int? MatchWindowMinutes,
    int? CommunityWindowMinutes)
{
    /// <summary>
    /// Most specific wins, and <c>null</c> means "ask the level above". Sessions sit between the
    /// match and the community once they have a surface of their own (v0.5); adding them is a term
    /// in this expression.
    /// </summary>
    public TimeSpan Window =>
        (MatchWindowMinutes ?? CommunityWindowMinutes) is { } minutes
            ? TimeSpan.FromMinutes(minutes)
            : CommunityPolicies.DefaultAmendWindow;
}

/// <summary>
/// Amending a result is not editing a row: it hands back the rating the match took and applies the
/// new one, so it moves other people's ladder positions. Organisers may do it because running
/// matches is their job; everyone else may only fix what they themselves played, and only while it
/// is still fresh enough for the reversal to be exact.
/// </summary>
public sealed record AmendMatchRequirement : IAuthorizationRequirement;

public sealed class AmendMatchHandler(CurrentPlayerAccessor current, TimeProvider clock)
    : AuthorizationHandler<AmendMatchRequirement, MatchAmendContext>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AmendMatchRequirement requirement,
        MatchAmendContext resource)
    {
        if (await current.GetAsync() is not { IsActiveMember: true, Role: { } role } me
            || me.CommunityId != resource.CommunityId)
        {
            return;
        }

        if (role >= CommunityRole.Organizer)
        {
            context.Succeed(requirement);

            return;
        }

        // A window of nothing is closed, not open-until-a-moment-passes: zero is how a group says
        // corrections are an organiser's alone.
        if (resource.Window > TimeSpan.Zero
            && resource.ParticipantPlayerIds.Contains(me.PlayerId)
            && clock.GetUtcNow() - resource.RecordedAt <= resource.Window)
        {
            context.Succeed(requirement);
        }
    }
}
