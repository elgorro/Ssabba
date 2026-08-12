namespace Ssabba.Domain.Entities;

/// <summary>
/// A community's house rules for one format. Sessions, matches and tournaments point at one; a
/// played match also copies the values down, so amending a rule set never rewrites past results.
/// </summary>
public class RuleSet
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public Guid FormatId { get; set; }
    public Format? Format { get; set; }

    public required string Name { get; set; }

    public int SetsToWin { get; set; }

    public int PointsPerSet { get; set; }

    public int WinBy { get; set; } = 2;

    public int TiebreakPoints { get; set; }

    /// <summary>Swap ends every N points, or <c>null</c> when the group does not bother.</summary>
    public int? SwitchEveryPoints { get; set; }

    public bool LetServeAllowed { get; set; } = true;

    /// <summary>The rule set applied when nobody picks one. At most one per community and format.</summary>
    public bool IsDefault { get; set; }

    public string? Notes { get; set; }
}
