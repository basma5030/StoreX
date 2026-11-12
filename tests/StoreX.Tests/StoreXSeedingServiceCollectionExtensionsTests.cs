
using Microsoft.Extensions.DependencyInjection;
using StoreX.Api.Infrastructure.Seeding;

namespace StoreX.Tests;

public class StoreXSeedingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStoreXSeeder_ShouldRegisterSeederAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returnedServices =
            services.AddStoreXSeeder<TestSeeder>();

        using var serviceProvider = services.BuildServiceProvider();

        using var firstScope = serviceProvider.CreateScope();
        using var secondScope = serviceProvider.CreateScope();

        var firstSeeder = firstScope.ServiceProvider
            .GetRequiredService<IStoreXSeeder>();

        var sameScopeSeeder = firstScope.ServiceProvider
            .GetRequiredService<IStoreXSeeder>();

        var secondScopeSeeder = secondScope.ServiceProvider
            .GetRequiredService<IStoreXSeeder>();

        // Assert
        Assert.Same(services, returnedServices);

        Assert.NotNull(firstSeeder);

        Assert.IsType<TestSeeder>(firstSeeder);

        Assert.Same(firstSeeder, sameScopeSeeder);

        Assert.NotSame(firstSeeder, secondScopeSeeder);
    }

    public sealed class TestSeeder : IStoreXSeeder
    {
        public Task SeedAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
