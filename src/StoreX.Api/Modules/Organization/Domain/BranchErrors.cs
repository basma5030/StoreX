using StoreX.Api.Shared;

namespace StoreX.Api.Modules.Organization.Domain;

public static class BranchErrors
{
    public const string NotFoundCode = "Organization.Branch.NotFound";
    public const string DuplicateCodeCode = "Organization.Branch.DuplicateCode";
    public const string ValidationCode = "Organization.Branch.Validation";

    public static readonly Error NotFound =
        new(NotFoundCode, "Branch was not found.");

    public static readonly Error DuplicateCode =
        new(DuplicateCodeCode, "A branch with this code already exists.");
}
