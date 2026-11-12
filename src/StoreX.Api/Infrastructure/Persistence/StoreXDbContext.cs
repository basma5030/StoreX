using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

using StoreX.Api.Infrastructure.Persistence.Auditing;
using StoreX.Api.Modules.Organization.Entities;
using StoreX.Api.Shared;

namespace StoreX.Api.Infrastructure.Persistence;

public sealed class StoreXDbContext : DbContext
{
    public StoreXDbContext(
        DbContextOptions<StoreXDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StoreXDbContext).Assembly);

        // Apply a global soft-delete filter to all applicable entities.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.BaseType is not null)
                continue;

            var parameter = Expression.Parameter(
                entityType.ClrType,
                "entity");

            var isDeletedProperty = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                new[] { typeof(bool) },
                parameter,
                Expression.Constant(nameof(ISoftDeletable.IsDeleted)));

            var filter = Expression.Lambda(
                Expression.Not(isDeletedProperty),
                parameter);

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }
    }
}
