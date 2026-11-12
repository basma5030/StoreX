using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StoreX.Api.Infrastructure.Persistence;
using StoreX.Api.Infrastructure.Persistence.Auditing;

namespace StoreX.Tests;

public sealed class StoreXApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"StoreX.Tests.{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:StoreX"] = "Testing"
                });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<StoreXDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<StoreXDbContext>>();
            services.RemoveAll<StoreXDbContext>();

            services.AddDbContext<StoreXDbContext>(
                (serviceProvider, options) =>
                {
                    options.UseInMemoryDatabase(_databaseName);

                    options.AddInterceptors(
                        serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
                });
        });
    }
}
