namespace StoreX.Api.Modules.Organization.Contracts;

public interface IBranchLookup
{
    Task<BranchLookupResponse?> FindByIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);
}
