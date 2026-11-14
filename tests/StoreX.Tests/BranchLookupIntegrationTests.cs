using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StoreX.Api.Modules.Organization.Application;
using StoreX.Api.Modules.Organization.Contracts;

namespace StoreX.Tests;

public class BranchLookupIntegrationTests
{
    [Fact]
    public void BranchLookup_AndBranchService_ShouldUseSameInstance()
    {
        using var factory = new StoreXApiFactory();
        using var scope = factory.Services.CreateScope();

        var branchService =
            scope.ServiceProvider.GetRequiredService<IBranchService>();

        var branchLookup =
            scope.ServiceProvider.GetRequiredService<IBranchLookup>();

        Assert.Same(branchService, branchLookup);
    }

    [Fact]
    public async Task FindByIdAsync_WhenBranchExists_ShouldReturnBranchData()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var branchId = await CreateBranchAsync(client, "BR-LOOKUP-01");

        using var scope = factory.Services.CreateScope();

        var branchLookup =
            scope.ServiceProvider.GetRequiredService<IBranchLookup>();

        var result = await branchLookup.FindByIdAsync(branchId);

        Assert.NotNull(result);
        Assert.Equal(branchId, result.Id);
        Assert.Equal("BR-LOOKUP-01", result.Code);
        Assert.Equal("Test Branch", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task FindByIdAsync_WhenBranchIsSoftDeleted_ShouldReturnNull()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var branchId = await CreateBranchAsync(client, "BR-LOOKUP-02");

        var deleteResponse = await client.DeleteAsync(
            $"/api/branches/{branchId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var scope = factory.Services.CreateScope();

        var branchLookup =
            scope.ServiceProvider.GetRequiredService<IBranchLookup>();

        var result = await branchLookup.FindByIdAsync(branchId);

        Assert.Null(result);
    }

    private static async Task<Guid> CreateBranchAsync(
        HttpClient client,
        string code)
    {
        var response = await client.PostAsJsonAsync(
            "/api/branches",
            new
            {
                Code = code,
                Name = "Test Branch",
                Address = "Test Address",
                PhoneNumber = "01000000000"
            });

        response.EnsureSuccessStatusCode();

        var branch = await response.Content.ReadFromJsonAsync<BranchResponse>();

        Assert.NotNull(branch);

        return branch.Id;
    }
}
