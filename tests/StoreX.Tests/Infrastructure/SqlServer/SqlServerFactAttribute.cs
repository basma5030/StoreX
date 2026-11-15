using Xunit;

namespace StoreX.Tests.Infrastructure.SqlServer;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("STOREX_SQLSERVER_TEST_CONNECTION")))
        {
            Skip = "Set STOREX_SQLSERVER_TEST_CONNECTION to run SQL Server integration tests.";
        }
    }
}
