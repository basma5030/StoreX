using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreX.Api.Modules.Organization.Entities;
using StoreX.Tests.Infrastructure.SqlServer;
using Xunit;

namespace StoreX.Tests.Integration.SqlServer;

[Collection(SqlServerTestCollection.Name)]
public sealed class BranchSqlServerTests
{
    private readonly SqlServerTestFixture _fixture;

    public BranchSqlServerTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SqlServerFact]
    public async Task Database_Should_Reject_Duplicate_Branch_Codes()
    {
        await using var scope = _fixture.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<StoreX.Api.Infrastructure.Persistence.StoreXDbContext>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        var firstBranch = new Branch(
            "BR001",
            "First Branch",
            "First Address");

        dbContext.Branches.Add(firstBranch);
        await dbContext.SaveChangesAsync();

        var duplicateBranch = new Branch(
            "BR001",
            "Second Branch",
            "Second Address");

        dbContext.Branches.Add(duplicateBranch);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());

        await transaction.RollbackAsync();
    }

    [SqlServerFact]
    public async Task Database_Should_Reject_Reusing_Soft_Deleted_Branch_Code()
    {
        await using var scope = _fixture.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<StoreX.Api.Infrastructure.Persistence.StoreXDbContext>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        var branch = new Branch(
            "BR002",
            "Original Branch",
            "Original Address");

        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync();

        dbContext.Branches.Remove(branch);
        await dbContext.SaveChangesAsync();

        var replacementBranch = new Branch(
            "BR002",
            "Replacement Branch",
            "Replacement Address");

        dbContext.Branches.Add(replacementBranch);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());

        await transaction.RollbackAsync();
    }
}
