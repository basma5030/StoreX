namespace StoreX.Api.Modules.Organization.Contracts;

public sealed record UpdateBranchRequest(
    string? Name,
    string? Address,
    string? PhoneNumber);
