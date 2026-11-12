
using StoreX.Api.Shared;

namespace StoreX.Tests;

public class AuditableEntityTests
{
    private sealed class TestAuditableEntity : AuditableEntity
    {
    }

    [Fact]
    public void AuditableEntity_ShouldInheritIdAndStoreAuditInformation()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var createdAt = DateTime.UtcNow;

        // Act
        entity.CreatedAtUtc = createdAt;
        entity.CreatedBy = "user-001";
        entity.UpdatedAtUtc = createdAt.AddHours(1);
        entity.UpdatedBy = "user-002";

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(createdAt, entity.CreatedAtUtc);
        Assert.Equal("user-001", entity.CreatedBy);
        Assert.Equal(createdAt.AddHours(1), entity.UpdatedAtUtc);
        Assert.Equal("user-002", entity.UpdatedBy);
    }
    [Fact]
    public void AuditableEntity_ShouldHaveNullUpdateInformationInitially()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Assert
        Assert.Null(entity.UpdatedAtUtc);
        Assert.Null(entity.UpdatedBy);
    }
}
