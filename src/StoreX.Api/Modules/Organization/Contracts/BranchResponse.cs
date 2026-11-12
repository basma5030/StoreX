namespace StoreX.Api.Modules.Organization.Contracts;

public sealed record BranchResponse(
    Guid Id,
    string Code,
    string Name,
    string Address,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
