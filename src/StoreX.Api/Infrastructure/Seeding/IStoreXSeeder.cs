namespace StoreX.Api.Infrastructure.Seeding;

public interface IStoreXSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}