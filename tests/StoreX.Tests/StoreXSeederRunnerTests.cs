using Microsoft.Extensions.Logging;
using StoreX.Api.Infrastructure.Seeding;

namespace StoreX.Tests;

public class StoreXSeederRunnerTests
{
    [Fact]
    public async Task SeedAllAsync_ShouldExecuteAllSeedersInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();

        var firstSeeder = new TestSeeder(
            "FirstSeeder",
            executionOrder);

        var secondSeeder = new TestSeeder(
            "SecondSeeder",
            executionOrder);

        var seeders = new IStoreXSeeder[]
        {
            firstSeeder,
            secondSeeder
        };

        using var loggerFactory = LoggerFactory.Create(
            builder => builder.SetMinimumLevel(LogLevel.Debug));

        var logger = loggerFactory.CreateLogger<StoreXSeederRunner>();

        var runner = new StoreXSeederRunner(seeders, logger);

        // Act
        await runner.SeedAllAsync();

        // Assert
        Assert.Equal(
            new[] { "FirstSeeder", "SecondSeeder" },
            executionOrder);
    }

    [Fact]
    public async Task SeedAllAsync_WhenSeederFails_ShouldStopExecutionAndThrow()
    {
        // Arrange
        var executionOrder = new List<string>();

        var failingSeeder = new TestSeeder(
            "FailingSeeder",
            executionOrder,
            shouldFail: true);

        var secondSeeder = new TestSeeder(
            "SecondSeeder",
            executionOrder);

        var seeders = new IStoreXSeeder[]
        {
            failingSeeder,
            secondSeeder
        };

        using var loggerFactory = LoggerFactory.Create(
            builder => builder.SetMinimumLevel(LogLevel.Debug));

        var logger = loggerFactory.CreateLogger<StoreXSeederRunner>();

        var runner = new StoreXSeederRunner(seeders, logger);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.SeedAllAsync());

        Assert.Equal(
            new[] { "FailingSeeder" },
            executionOrder);
    }

    private sealed class TestSeeder : IStoreXSeeder
    {
        private readonly string _name;
        private readonly List<string> _executionOrder;
        private readonly bool _shouldFail;

        public TestSeeder(
            string name,
            List<string> executionOrder,
            bool shouldFail = false)
        {
            _name = name;
            _executionOrder = executionOrder;
            _shouldFail = shouldFail;
        }

        public Task SeedAsync(
            CancellationToken cancellationToken = default)
        {
            _executionOrder.Add(_name);

            if (_shouldFail)
            {
                throw new InvalidOperationException(
                    "Seeder test failure.");
            }

            return Task.CompletedTask;
        }
    }
}