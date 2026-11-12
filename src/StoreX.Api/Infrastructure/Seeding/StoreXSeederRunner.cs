using Microsoft.Extensions.Logging;

namespace StoreX.Api.Infrastructure.Seeding;

public sealed class StoreXSeederRunner
{
    private readonly IEnumerable<IStoreXSeeder> _seeders;
    private readonly ILogger<StoreXSeederRunner> _logger;

    public StoreXSeederRunner(
        IEnumerable<IStoreXSeeder> seeders,
        ILogger<StoreXSeederRunner> logger)
    {
        _seeders = seeders;
        _logger = logger;
    }

    public async Task SeedAllAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var seeder in _seeders)
        {
            var seederName = seeder.GetType().Name;

            _logger.LogInformation(
                "Starting seeder: {SeederName}",
                seederName);

            try
            {
                await seeder.SeedAsync(cancellationToken);

                _logger.LogInformation(
                    "Seeder completed successfully: {SeederName}",
                    seederName);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Seeder failed: {SeederName}",
                    seederName);

                throw;
            }
        }
    }
}