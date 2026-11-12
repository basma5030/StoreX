using Microsoft.Extensions.DependencyInjection;

namespace StoreX.Api.Infrastructure.Seeding;

public static class StoreXSeedingServiceCollectionExtensions
{
    public static IServiceCollection AddStoreXSeeder<TSeeder>(
        this IServiceCollection services)
        where TSeeder : class, IStoreXSeeder
    {
        services.AddScoped<IStoreXSeeder, TSeeder>();

        return services;
    }
}