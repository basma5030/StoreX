
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StoreX.Api.Shared;

namespace StoreX.Api.Infrastructure.Persistence.Auditing;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public AuditSaveChangesInterceptor(
        IClock clock,
        ICurrentUser currentUser)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        PrepareAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PrepareAuditEntries(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void PrepareAuditEntries(DbContext? context)
    {
        if (context is null)
            return;

        context.ChangeTracker.DetectChanges();

        var utcNow = _clock.UtcNow;
        var userId = _currentUser.UserId;
        var branchId = _currentUser.BranchId;

        var auditLogs = new List<AuditLog>();

        var entries = context.ChangeTracker
            .Entries()
            .Where(entry =>
                entry.Entity is not AuditLog &&
                (entry.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var originalState = entry.State;

            var isSoftDelete =
                originalState == EntityState.Deleted &&
                entry.Entity is ISoftDeletable;

            if (isSoftDelete)
            {
                // Convert Deleted state to Modified first.
                entry.State = EntityState.Modified;

                // Clear modified flags before changing property values.
                foreach (var property in entry.Properties)
                    property.IsModified = false;

                var softDeletable = (ISoftDeletable)entry.Entity;

                softDeletable.IsDeleted = true;
                softDeletable.DeletedAtUtc = utcNow;
                softDeletable.DeletedBy = userId?.ToString();

                // Mark only soft-delete properties as modified.
                entry.Property(nameof(ISoftDeletable.IsDeleted))
                    .IsModified = true;

                entry.Property(nameof(ISoftDeletable.DeletedAtUtc))
                    .IsModified = true;

                entry.Property(nameof(ISoftDeletable.DeletedBy))
                    .IsModified = true;
            }

            if (entry.Entity is AuditableEntity auditable)
            {
                if (originalState == EntityState.Added)
                {
                    auditable.CreatedAtUtc = utcNow;
                    auditable.CreatedBy = userId?.ToString();

                    auditable.UpdatedAtUtc = null;
                    auditable.UpdatedBy = null;
                }
                else if (originalState == EntityState.Modified || isSoftDelete)
                {
                    entry.Property(nameof(AuditableEntity.CreatedAtUtc))
                        .IsModified = false;

                    entry.Property(nameof(AuditableEntity.CreatedBy))
                        .IsModified = false;

                    auditable.UpdatedAtUtc = utcNow;
                    auditable.UpdatedBy = userId?.ToString();

                    entry.Property(nameof(AuditableEntity.UpdatedAtUtc))
                        .IsModified = true;

                    entry.Property(nameof(AuditableEntity.UpdatedBy))
                        .IsModified = true;
                }
            }

            var action = originalState switch
            {
                EntityState.Added => "Added",
                EntityState.Modified => "Modified",
                EntityState.Deleted => "Deleted",
                _ => null
            };

            if (action is null)
                continue;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (IsSensitiveProperty(propertyName))
                    continue;

                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (originalState == EntityState.Added)
                {
                    newValues[propertyName] = property.CurrentValue;
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (!property.IsModified)
                        continue;

                    oldValues[propertyName] = property.OriginalValue;
                    newValues[propertyName] = property.CurrentValue;
                }
                else if (originalState == EntityState.Deleted)
                {
                    oldValues[propertyName] = property.OriginalValue;
                }
            }

            auditLogs.Add(new AuditLog
            {
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = (Guid)entry.Property("Id").CurrentValue!,
                Action = action,
                OldValues = oldValues.Count == 0
                    ? null
                    : JsonSerializer.Serialize(oldValues),
                NewValues = newValues.Count == 0
                    ? null
                    : JsonSerializer.Serialize(newValues),
                ChangedAtUtc = utcNow,
                UserId = userId,
                BranchId = branchId
            });
        }

        if (auditLogs.Count > 0)
            context.Set<AuditLog>().AddRange(auditLogs);
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        string[] sensitiveTerms =
        [
            "Password",
            "Token",
            "Secret",
            "ApiKey",
            "SecurityStamp",
            "Otp",
            "Pin"
        ];

        return sensitiveTerms.Any(term =>
            propertyName.Contains(
                term,
                StringComparison.OrdinalIgnoreCase));
    }
}
