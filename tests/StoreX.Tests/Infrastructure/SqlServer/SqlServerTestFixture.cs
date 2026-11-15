
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreX.Api.Infrastructure;
using StoreX.Api.Infrastructure.Persistence;
using StoreX.Api.Infrastructure.Persistence.Auditing;
using StoreX.Api.Shared;

namespace StoreX.Tests.Infrastructure.SqlServer;

public sealed class SqlServerTestFixture : IAsyncLifetime
{
    private const string ExpectedDatabaseName = "StoreX_IntegrationTests";

    private readonly string? _connectionString =
        Environment.GetEnvironmentVariable("STOREX_SQLSERVER_TEST_CONNECTION");

    private ServiceProvider? _serviceProvider;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public AsyncServiceScope CreateScope()
    {
        return _serviceProvider?.CreateAsyncScope()
            ?? throw new InvalidOperationException(
                "SQL Server fixture has not been initialized.");
    }

    public async Task InitializeAsync()
    {
        if (!IsConfigured)
            return;

        var connectionStringBuilder =
            new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_connectionString);

        if (!string.Equals(
                connectionStringBuilder.InitialCatalog,
                ExpectedDatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"SQL Server integration tests must use '{ExpectedDatabaseName}'.");
        }

        var services = new ServiceCollection();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<StoreXDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(_connectionString);

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        _serviceProvider = services.BuildServiceProvider();

        await using var scope = _serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<StoreXDbContext>();

        await dbContext.Database.MigrateAsync();

        var appliedMigrations =
            await dbContext.Database.GetAppliedMigrationsAsync();

        var expectedMigrations = new[]
        {
            "20261112165445_InitialCreate",
            "20261112205810_AddAuditLogs"
        };

        var missingMigrations = expectedMigrations
            .Except(appliedMigrations, StringComparer.Ordinal)
            .ToArray();

        if (missingMigrations.Length > 0)
        {
            throw new InvalidOperationException(
                $"Missing SQL Server migrations: {string.Join(", ", missingMigrations)}");
        }
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
            await _serviceProvider.DisposeAsync();
    }
}




