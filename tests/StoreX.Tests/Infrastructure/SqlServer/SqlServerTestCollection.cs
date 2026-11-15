using Xunit;

namespace StoreX.Tests.Infrastructure.SqlServer;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SqlServerTestCollection : ICollectionFixture<SqlServerTestFixture>
{
    public const string Name = "SQL Server integration tests";
}
