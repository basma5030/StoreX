
namespace StoreX.Api.Shared;

public sealed record PagedRequest
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public int SafePageNumber =>
        Math.Max(1, PageNumber);

    public int SafePageSize =>
        Math.Clamp(PageSize, 1, 100);

    public int Skip
    {
        get
        {
            var skip = (long)(SafePageNumber - 1) * SafePageSize;

            return (int)Math.Min(skip, int.MaxValue);
        }
    }
}
