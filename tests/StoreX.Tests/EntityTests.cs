using StoreX.Api.Shared;

namespace StoreX.Tests;

public class EntityTests
{
    private sealed class TestEntity : Entity
    {
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var firstEntity = new TestEntity();
        var secondEntity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, firstEntity.Id);
        Assert.NotEqual(firstEntity.Id, secondEntity.Id);
    }
}
