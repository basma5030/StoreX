
using StoreX.Api.Shared;

namespace StoreX.Tests;

public class SoftDeletableTests
{
    private sealed class TestDeletableEntity : ISoftDeletable
    {
        public bool IsDeleted { get; set; }

        public DateTime? DeletedAtUtc { get; set; }

        public string? DeletedBy { get; set; }
    }

    [Fact]
    public void SoftDelete_ShouldStoreDeletionInformation()
    {
        // Arrange
        var entity = new TestDeletableEntity();
        var deletedAt = DateTime.UtcNow;

        // Act
        entity.IsDeleted = true;
        entity.DeletedAtUtc = deletedAt;
        entity.DeletedBy = "user-001";

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedAt, entity.DeletedAtUtc);
        Assert.Equal("user-001", entity.DeletedBy);
    }
    [Fact]
    public void SoftDeletable_ShouldStartAsNotDeleted()
    {
        // Arrange
        var entity = new TestDeletableEntity();

        // Assert
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAtUtc);
        Assert.Null(entity.DeletedBy);
    }
}
