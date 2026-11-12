
using StoreX.Api.Shared;

namespace StoreX.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldReturnFailedResultWithError()
    {
        var error = new Error(
            "Catalog.DuplicateItemCode",
            "Item code already exists.");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_ShouldRejectNullError()
    {
        Assert.Throws<ArgumentNullException>(
            () => Result.Failure(null!));
    }

    [Fact]
    public void Failure_ShouldRejectNoneError()
    {
        Assert.Throws<ArgumentException>(
            () => Result.Failure(Error.None));
    }

    [Fact]
    public void GenericSuccess_ShouldReturnSuccessfulResultWithValue()
    {
        var result = Result<string>.Success("Laptop");

        Assert.True(result.IsSuccess);
        Assert.Equal("Laptop", result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void GenericSuccess_ShouldRejectNullValue()
    {
        Assert.Throws<ArgumentNullException>(
            () => Result<string?>.Success(null));
    }

    [Fact]
    public void GenericFailure_ShouldReturnFailedResultWithoutValue()
    {
        var error = new Error(
            "Catalog.ItemNotFound",
            "Item was not found.");

        var result = Result<string>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void GenericFailure_ShouldRejectNullError()
    {
        Assert.Throws<ArgumentNullException>(
            () => Result<string>.Failure(null!));
    }

    [Fact]
    public void GenericFailure_ShouldRejectNoneError()
    {
        Assert.Throws<ArgumentException>(
            () => Result<string>.Failure(Error.None));
    }

    [Fact]
    public void Error_ShouldRejectNullCode()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Error(null!, "Description"));
    }

    [Fact]
    public void Error_ShouldRejectNullDescription()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Error("Test.Error", null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Error_ShouldRejectEmptyCode(string code)
    {
        Assert.Throws<ArgumentException>(
            () => new Error(code, "Description"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Error_ShouldRejectEmptyDescription(string description)
    {
        Assert.Throws<ArgumentException>(
            () => new Error("Test.Error", description));
    }
}
