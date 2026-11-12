
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StoreX.Api.Shared;

namespace StoreX.Api.Infrastructure;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId =>
        GetGuidClaim(ClaimTypes.NameIdentifier);

    public Guid? BranchId =>
        GetGuidClaim("BranchId");

    private Guid? GetGuidClaim(string claimType)
    {
        var value = _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirst(claimType)?
            .Value;

        return Guid.TryParse(value, out var id)
            ? id
            : null;
    }
}
