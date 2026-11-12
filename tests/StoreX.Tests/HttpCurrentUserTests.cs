
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StoreX.Api.Infrastructure;

namespace StoreX.Tests;

public class HttpCurrentUserTests
{
    [Fact]
    public void UserIdAndBranchId_ShouldReturnCurrentUserClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("BranchId", branchId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        var accessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var currentUser = new HttpCurrentUser(accessor);

        // Act & Assert
        Assert.Equal(userId, currentUser.UserId);
        Assert.Equal(branchId, currentUser.BranchId);
    }

    [Fact]
    public void UserIdAndBranchId_ShouldReturnNullWhenClaimsAreMissing()
    {
        // Arrange
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        var currentUser = new HttpCurrentUser(accessor);

        // Act & Assert
        Assert.Null(currentUser.UserId);
        Assert.Null(currentUser.BranchId);
    }

    [Fact]
    public void UserIdAndBranchId_ShouldReturnNullWhenClaimsAreInvalid()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-user-id"),
            new Claim("BranchId", "invalid-branch-id")
        };

        var identity = new ClaimsIdentity(claims);
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var currentUser = new HttpCurrentUser(accessor);

        // Act & Assert
        Assert.Null(currentUser.UserId);
        Assert.Null(currentUser.BranchId);
    }

    [Fact]
    public void UserIdAndBranchId_ShouldReturnNullWhenHttpContextIsMissing()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var currentUser = new HttpCurrentUser(accessor);

        // Act & Assert
        Assert.Null(currentUser.UserId);
        Assert.Null(currentUser.BranchId);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenHttpContextAccessorIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new HttpCurrentUser(null!));
    }
}