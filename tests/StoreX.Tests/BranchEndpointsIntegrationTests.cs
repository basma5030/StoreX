using System.Net;
using System.Net.Http.Json;
using StoreX.Api.Modules.Organization.Contracts;

namespace StoreX.Tests;

public class BranchEndpointsIntegrationTests
{
    [Fact]
    public async Task CreateBranch_ShouldReturnCreatedAndNormalizedCode()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/branches",
            new
            {
                Code = " cairo-01 ",
                Name = "Cairo Branch",
                Address = "Cairo",
                PhoneNumber = "01000000000"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var branch = await response.Content.ReadFromJsonAsync<BranchResponse>();

        Assert.NotNull(branch);
        Assert.Equal("CAIRO-01", branch.Code);
        Assert.Equal("Cairo Branch", branch.Name);
    }

    [Fact]
    public async Task CreateBranch_WithDuplicateCode_ShouldReturnConflict()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        await CreateBranchAsync(client, "BR-001");

        var response = await client.PostAsJsonAsync(
            "/api/branches",
            new
            {
                Code = "br-001",
                Name = "Another Branch",
                Address = "Alexandria",
                PhoneNumber = "01000000000"
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetAllBranches_ShouldReturnCreatedBranches()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        await CreateBranchAsync(client, "BR-001");
        await CreateBranchAsync(client, "BR-002");

        var response = await client.GetAsync(
            "/api/branches?pageNumber=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("BR-001", content);
        Assert.Contains("BR-002", content);
    }

    [Fact]
    public async Task GetBranchById_WhenBranchDoesNotExist_ShouldReturnNotFound()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/branches/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBranch_ShouldUpdateItsDetails()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var id = await CreateBranchAsync(client, "BR-001");

        var response = await client.PutAsJsonAsync(
            $"/api/branches/{id}",
            new
            {
                Name = "Updated Branch",
                Address = "New Cairo",
                PhoneNumber = "01111111111"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var branch = await response.Content.ReadFromJsonAsync<BranchResponse>();

        Assert.NotNull(branch);
        Assert.Equal("Updated Branch", branch.Name);
        Assert.Equal("New Cairo", branch.Address);
    }

    [Fact]
    public async Task ChangeBranchStatus_ShouldUpdateActiveStatus()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var id = await CreateBranchAsync(client, "BR-001");

        var response = await client.PatchAsJsonAsync(
            $"/api/branches/{id}/status",
            new { IsActive = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var branch = await response.Content.ReadFromJsonAsync<BranchResponse>();

        Assert.NotNull(branch);
        Assert.False(branch.IsActive);
    }

    [Fact]
    public async Task DeleteBranch_ShouldSoftDeleteAndHideItFromQueries()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var id = await CreateBranchAsync(client, "BR-001");

        var deleteResponse = await client.DeleteAsync(
            $"/api/branches/{id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/api/branches/{id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
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
