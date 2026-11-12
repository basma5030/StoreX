using Microsoft.EntityFrameworkCore;
using StoreX.Api.Modules.Organization.Contracts;
using StoreX.Api.Modules.Organization.Domain;
using StoreX.Api.Modules.Organization.Entities;
using StoreX.Api.Infrastructure.Persistence;
using StoreX.Api.Shared;

namespace StoreX.Api.Modules.Organization.Application;

public sealed class BranchService : IBranchService
{
    private readonly StoreXDbContext _dbContext;

    public BranchService(StoreXDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<BranchResponse>> CreateAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateDetails(
            request.Name, request.Address, request.PhoneNumber);

        if (validationError is not null)
            return Result<BranchResponse>.Failure(validationError);

        if (string.IsNullOrWhiteSpace(request.Code))
            return ValidationFailure<BranchResponse>("Branch code is required.");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (normalizedCode.Length > 20)
            return ValidationFailure<BranchResponse>(
                "Branch code cannot exceed 20 characters.");

        var exists = await _dbContext.Branches
            .IgnoreQueryFilters()
            .AnyAsync(branch => branch.Code == normalizedCode, cancellationToken);

        if (exists)
            return Result<BranchResponse>.Failure(BranchErrors.DuplicateCode);

        var branch = new Branch(
            normalizedCode,
            request.Name!,
            request.Address!,
            request.PhoneNumber);

        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<BranchResponse>.Success(ToResponse(branch));
    }

    public async Task<PagedResult<BranchResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Branches
            .AsNoTracking()
            .OrderBy(branch => branch.Code);

        var totalCount = await query.CountAsync(cancellationToken);

        var branches = await query
            .Skip(request.Skip)
            .Take(request.SafePageSize)
            .ToListAsync(cancellationToken);

        var items = branches.Select(ToResponse).ToList();

        return new PagedResult<BranchResponse>(
            items,
            request.SafePageNumber,
            request.SafePageSize,
            totalCount);
    }

    public async Task<Result<BranchResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(branch => branch.Id == id, cancellationToken);

        if (branch is null)
            return Result<BranchResponse>.Failure(BranchErrors.NotFound);

        return Result<BranchResponse>.Success(ToResponse(branch));
    }

    public async Task<Result<BranchResponse>> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(branch => branch.Id == id, cancellationToken);

        if (branch is null)
            return Result<BranchResponse>.Failure(BranchErrors.NotFound);

        var validationError = ValidateDetails(
            request.Name, request.Address, request.PhoneNumber);

        if (validationError is not null)
            return Result<BranchResponse>.Failure(validationError);

        branch.UpdateDetails(
            request.Name!,
            request.Address!,
            request.PhoneNumber);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<BranchResponse>.Success(ToResponse(branch));
    }

    public async Task<Result<BranchResponse>> ChangeStatusAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(branch => branch.Id == id, cancellationToken);

        if (branch is null)
            return Result<BranchResponse>.Failure(BranchErrors.NotFound);

        branch.SetActive(isActive);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<BranchResponse>.Success(ToResponse(branch));
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(branch => branch.Id == id, cancellationToken);

        if (branch is null)
            return Result.Failure(BranchErrors.NotFound);

        _dbContext.Branches.Remove(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Error? ValidateDetails(
        string? name,
        string? address,
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new Error(BranchErrors.ValidationCode, "Branch name is required.");

        if (name.Trim().Length > 150)
            return new Error(BranchErrors.ValidationCode,
                "Branch name cannot exceed 150 characters.");

        if (string.IsNullOrWhiteSpace(address))
            return new Error(BranchErrors.ValidationCode, "Branch address is required.");

        if (address.Trim().Length > 300)
            return new Error(BranchErrors.ValidationCode,
                "Branch address cannot exceed 300 characters.");

        if (phoneNumber?.Trim().Length > 30)
            return new Error(BranchErrors.ValidationCode,
                "Branch phone number cannot exceed 30 characters.");

        return null;
    }

    private static Result<T> ValidationFailure<T>(string description)
    {
        return Result<T>.Failure(
            new Error(BranchErrors.ValidationCode, description));
    }

    private static BranchResponse ToResponse(Branch branch)
    {
        return new BranchResponse(
            branch.Id,
            branch.Code,
            branch.Name,
            branch.Address,
            branch.PhoneNumber,
            branch.IsActive,
            branch.CreatedAtUtc,
            branch.UpdatedAtUtc);
    }
}
