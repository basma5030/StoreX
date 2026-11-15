using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreX.Tests.Infrastructure.SqlServer;
using Xunit;

namespace StoreX.Tests.Integration.SqlServer;

[Collection(SqlServerTestCollection.Name)]
public sealed class SqlServerMigrationsTests
{
    private readonly SqlServerTestFixture _fixture;

    public SqlServerMigrationsTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SqlServerFact]
    public async Task Database_Should_Have_All_Expected_Migrations()
    {
        await using var scope = _fixture.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<StoreX.Api.Infrastructure.Persistence.StoreXDbContext>();

        var appliedMigrations =
            await dbContext.Database.GetAppliedMigrationsAsync();

        Assert.Contains("20261112165445_InitialCreate", appliedMigrations);
        Assert.Contains("20261112205810_AddAuditLogs", appliedMigrations);
    }
}
