using System.Net;

namespace StoreX.Tests;

public sealed class HealthCheckIntegrationTests
{
    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        using var factory = new StoreXApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
