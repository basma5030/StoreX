
namespace StoreX.Api.Shared;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage =>
        PageNumber > 1;

    public bool HasNextPage =>
        PageNumber < TotalPages;

    public PagedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        if (totalCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCount));

        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
