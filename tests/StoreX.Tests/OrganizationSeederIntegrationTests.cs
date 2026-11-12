
using Microsoft.Extensions.DependencyInjection;
using StoreX.Api.Infrastructure.Seeding;
using StoreX.Api.Modules.Organization.Seeding;

namespace StoreX.Tests;

public class OrganizationSeederIntegrationTests
{
    [Fact]
    public async Task OrganizationSeeder_ShouldBeResolvedAndExecutedByRunner()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddStoreXSeeder<OrganizationSeeder>();

        services.AddScoped<StoreXSeederRunner>();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();

        var seeders = scope.ServiceProvider
            .GetServices<IStoreXSeeder>()
            .ToList();

        var runner = scope.ServiceProvider
            .GetRequiredService<StoreXSeederRunner>();

        // Assert
        var seeder = Assert.Single(seeders);

        Assert.IsType<OrganizationSeeder>(seeder);

        await runner.SeedAllAsync();
    }
}
