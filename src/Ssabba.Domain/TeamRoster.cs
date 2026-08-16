namespace Ssabba.Domain;

/// <summary>
/// The natural key of a lineup: which players are in it, independent of the order they were
/// entered in. Two teams in a community with the same key are the same team, which is what lets a
/// roster typed in at the net find the pairing that already played rather than make another one.
/// </summary>
public static class TeamRoster
{
    /// <summary>Matches the length of the <c>MemberKey</c> column: 28 teams of six would still fit.</summary>
    public const int MaxKeyLength = 1024;

    /// <summary>
    /// Sorts the player ids, drops repeats and joins them with hyphens. A team of one player listed
    /// twice is a team of one, and the caller is expected to have rejected that already.
    /// </summary>
    public static string Key(IEnumerable<Guid> playerIds)
    {
        ArgumentNullException.ThrowIfNull(playerIds);

        // Sorted on the hex text rather than on Guid, whose ordering is neither byte-wise nor the
        // one Postgres uses: the key has to come out the same in C# and in SQL.
        return string.Join(
            '-',
            playerIds.Distinct().Select(id => id.ToString("N")).OrderBy(hex => hex, StringComparer.Ordinal));
    }
}
