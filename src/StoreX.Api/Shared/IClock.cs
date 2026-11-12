
namespace StoreX.Api.Shared;

public interface IClock
{
    DateTime UtcNow { get; }
}
