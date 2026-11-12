
namespace StoreX.Api.Shared;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? BranchId { get; }
}
