using StoreX.Api.Modules.Organization.Contracts;
using StoreX.Api.Shared;

namespace StoreX.Api.Modules.Organization.Application;

public interface IBranchService
{
    Task<Result<BranchResponse>> CreateAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<BranchResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BranchResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<BranchResponse>> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BranchResponse>> ChangeStatusAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
