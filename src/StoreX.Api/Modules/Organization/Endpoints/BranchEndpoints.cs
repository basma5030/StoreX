using StoreX.Api.Modules.Organization.Application;
using StoreX.Api.Modules.Organization.Contracts;
using StoreX.Api.Modules.Organization.Domain;
using StoreX.Api.Shared;

namespace StoreX.Api.Modules.Organization.Endpoints;

public static class BranchEndpoints
{
    public static IEndpointRouteBuilder MapBranchEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/branches")
            .WithTags("Branches");

        group.MapPost("", CreateAsync);
        group.MapGet("", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapPatch("/{id:guid}/status", ChangeStatusAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateBranchRequest request,
        IBranchService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return ToProblem(result.Error);

        return Results.Created(
            $"/api/branches/{result.Value!.Id}",
            result.Value);
    }

    private static async Task<IResult> GetAllAsync(
        IBranchService service,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var request = new PagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await service.GetAllAsync(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IBranchService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        IBranchService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> ChangeStatusAsync(
        Guid id,
        ChangeBranchStatusRequest request,
        IBranchService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ChangeStatusAsync(
            id, request.IsActive, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IBranchService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error)
    {
        var statusCode = error.Code switch
        {
            BranchErrors.NotFoundCode => StatusCodes.Status404NotFound,
            BranchErrors.DuplicateCodeCode => StatusCodes.Status409Conflict,
            BranchErrors.ValidationCode => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            statusCode: statusCode,
            title: "Branch operation failed.",
            detail: error.Description,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code
            });
    }
}
