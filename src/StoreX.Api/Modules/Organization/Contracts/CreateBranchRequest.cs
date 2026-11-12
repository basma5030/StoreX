namespace StoreX.Api.Modules.Organization.Contracts;

public sealed record CreateBranchRequest(
    string? Code,
    string? Name,
    string? Address,
    string? PhoneNumber);
