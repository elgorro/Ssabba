namespace Ssabba.Shared;

/// <summary>One page of a longer list, together with what it takes to say "page 2 of 7".</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int PageCount => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
