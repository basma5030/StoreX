
using Microsoft.EntityFrameworkCore;
using StoreX.Api.Modules.Organization.Entities;
using StoreX.Api.Shared;

namespace StoreX.Api.Infrastructure.Persistence;

public sealed class StoreXDbContext : DbContext
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public StoreXDbContext(
        DbContextOptions<StoreXDbContext> options,
        IClock clock,
        ICurrentUser currentUser)
        : base(options)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    public DbSet<Branch> Branches => Set<Branch>();

    public override int SaveChanges(
        bool acceptAllChangesOnSuccess)
    {
        ApplyAuditAndSoftDelete();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void ApplyAuditAndSoftDelete()
    {
        var utcNow = _clock.UtcNow;
        var userId = _currentUser.UserId?.ToString();

        // Convert physical deletes into soft deletes.
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
                continue;

            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = utcNow;
            entry.Entity.DeletedBy = userId;

            entry.State = EntityState.Modified;
        }

        // Apply audit information.
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.CreatedBy = userId;

                entry.Entity.UpdatedAtUtc = null;
                entry.Entity.UpdatedBy = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAtUtc).IsModified = false;
                entry.Property(x => x.CreatedBy).IsModified = false;

                entry.Entity.UpdatedAtUtc = utcNow;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StoreXDbContext).Assembly);
    }
}
