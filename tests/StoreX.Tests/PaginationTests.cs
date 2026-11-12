
using StoreX.Api.Shared;

namespace StoreX.Tests;

public class PaginationTests
{
    [Fact]
    public void PagedRequest_ShouldUseDefaultValues()
    {
        var request = new PagedRequest();

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(20, request.PageSize);
        Assert.Equal(1, request.SafePageNumber);
        Assert.Equal(20, request.SafePageSize);
        Assert.Equal(0, request.Skip);
    }

    [Fact]
    public void PagedRequest_ShouldCalculateSkipCorrectly()
    {
        var request = new PagedRequest
        {
            PageNumber = 3,
            PageSize = 20
        };

        Assert.Equal(40, request.Skip);
    }

    [Fact]
    public void PagedRequest_ShouldNormalizeInvalidValues()
    {
        var request = new PagedRequest
        {
            PageNumber = 0,
            PageSize = 500
        };

        Assert.Equal(1, request.SafePageNumber);
        Assert.Equal(100, request.SafePageSize);
        Assert.Equal(0, request.Skip);
    }

    [Fact]
    public void PagedRequest_ShouldNormalizeNegativePageNumber()
    {
        var request = new PagedRequest
        {
            PageNumber = -5
        };

        Assert.Equal(1, request.SafePageNumber);
        Assert.Equal(0, request.Skip);
    }

    [Fact]
    public void PagedRequest_ShouldNormalizeNegativePageSize()
    {
        var request = new PagedRequest
        {
            PageSize = -10
        };

        Assert.Equal(1, request.SafePageSize);
    }

    [Fact]
    public void PagedRequest_ShouldPreventSkipOverflow()
    {
        var request = new PagedRequest
        {
            PageNumber = int.MaxValue,
            PageSize = 100
        };

        Assert.Equal(int.MaxValue, request.Skip);
    }

    [Fact]
    public void PagedResult_ShouldCalculatePaginationInformation()
    {
        var products = new List<string>
        {
            "Laptop",
            "Mouse",
            "Keyboard"
        };

        var result = new PagedResult<string>(
            products,
            pageNumber: 2,
            pageSize: 20,
            totalCount: 45);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(45, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_ShouldRejectInvalidPageNumber()
    {
        var products = new List<string>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PagedResult<string>(products, 0, 20, 100));
    }

    [Fact]
    public void PagedResult_ShouldRejectNullItems()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PagedResult<string>(null!, 1, 20, 100));
    }

    [Fact]
    public void PagedResult_ShouldRejectInvalidPageSize()
    {
        var products = new List<string>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PagedResult<string>(products, 1, 0, 100));
    }

    [Fact]
    public void PagedResult_ShouldRejectNegativeTotalCount()
    {
        var products = new List<string>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PagedResult<string>(products, 1, 20, -1));
    }

    [Fact]
    public void PagedResult_ShouldHandleEmptyResults()
    {
        var result = new PagedResult<string>(
            new List<string>(),
            pageNumber: 1,
            pageSize: 20,
            totalCount: 0);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_ShouldHandleSinglePage()
    {
        var result = new PagedResult<string>(
            new List<string> { "Laptop", "Mouse" },
            pageNumber: 1,
            pageSize: 20,
            totalCount: 2);

        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }
}