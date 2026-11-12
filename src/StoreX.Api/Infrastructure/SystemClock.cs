
using StoreX.Api.Shared;

namespace StoreX.Api.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
