
using StoreX.Api.Infrastructure.Seeding;

namespace StoreX.Api.Modules.Organization.Seeding;

public sealed class OrganizationSeeder : IStoreXSeeder
{
    public Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Organization reference data will be seeded here
        // when the required business data is available.

        return Task.CompletedTask;
    }
}
