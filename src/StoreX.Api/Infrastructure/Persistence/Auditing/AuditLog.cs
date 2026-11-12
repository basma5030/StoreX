using StoreX.Api.Shared;

namespace StoreX.Api.Infrastructure.Persistence.Auditing;

public sealed class AuditLog : Entity
{
    public string EntityName { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public Guid? UserId { get; set; }

    public Guid? BranchId { get; set; }
}
