namespace StoreX.Api.Modules.Organization.Contracts;

public sealed record BranchLookupResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive);
