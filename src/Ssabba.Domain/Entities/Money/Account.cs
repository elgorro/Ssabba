namespace Ssabba.Domain.Entities;

/// <summary>
/// A pot money sits in, or a member's running balance with the community. Every amount in the
/// ledger moves between two of these.
/// </summary>
public class Account
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }

    public required string Name { get; set; }

    public AccountKind Kind { get; set; }

    /// <summary>ISO-4217 code. All entries touching this account are in it.</summary>
    public required string Currency { get; set; }

    /// <summary>Set on <see cref="AccountKind.MemberBalance"/>: whose balance this is.</summary>
    public Guid? MemberId { get; set; }
    public CommunityMember? Member { get; set; }

    public bool IsActive { get; set; } = true;
}

public enum AccountKind
{
    Cash = 0,
    Bank = 1,

    /// <summary>What one member owes the community, or is owed by it.</summary>
    MemberBalance = 2,

    /// <summary>Money in from sponsors, grants and donations.</summary>
    Sponsorship = 3,

    /// <summary>Where costs are booked to.</summary>
    Expense = 4,
}
