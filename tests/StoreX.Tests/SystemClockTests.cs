
using StoreX.Api.Infrastructure;

namespace StoreX.Tests;

public class SystemClockTests
{
    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        var clock = new SystemClock();
        var before = DateTime.UtcNow;

        // Act
        var actual = clock.UtcNow;

        // Assert
        var after = DateTime.UtcNow;

        Assert.InRange(actual, before, after);
        Assert.Equal(DateTimeKind.Utc, actual.Kind);
    }
}
