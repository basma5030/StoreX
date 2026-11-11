using Microsoft.EntityFrameworkCore;

namespace StoreX.Api.Infrastructure.Persistence;

public sealed class StoreXDbContext : DbContext
{
    public StoreXDbContext(DbContextOptions<StoreXDbContext> options)
        : base(options)
    {
    }
}
