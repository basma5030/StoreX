
using Microsoft.EntityFrameworkCore;
using StoreX.Api.Infrastructure.Persistence;
using StoreX.Api.Modules.Organization.Entities;
using StoreX.Api.Shared;

namespace StoreX.Tests;

public class StoreXDbContextAuditTests
{
    [Fact]
    public void SaveChanges_WhenAddingBranch_ShouldSetCreationAudit()
    {
        var userId = Guid.NewGuid();
        var clock = new TestClock(
            new DateTime(2026, 11, 12, 16, 0, 0, DateTimeKind.Utc));

        var currentUser = new TestCurrentUser(userId);

        using var context = CreateContext(clock, currentUser);

        var branch = new Branch("BR-001", "Main Branch", "Cairo");

        context.Branches.Add(branch);
        context.SaveChanges();

        Assert.Equal(clock.UtcNow, branch.CreatedAtUtc);
        Assert.Equal(userId.ToString(), branch.CreatedBy);
        Assert.Null(branch.UpdatedAtUtc);
        Assert.Null(branch.UpdatedBy);
    }

    [Fact]
    public void SaveChanges_WhenUpdatingBranch_ShouldSetUpdateAudit()
    {
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();

        var clock = new TestClock(
            new DateTime(2026, 11, 12, 16, 0, 0, DateTimeKind.Utc));

        var currentUser = new TestCurrentUser(firstUserId);

        using var context = CreateContext(clock, currentUser);

        var branch = new Branch("BR-002", "Old Name", "Cairo");

        context.Branches.Add(branch);
        context.SaveChanges();

        var originalCreatedAt = branch.CreatedAtUtc;
        var originalCreatedBy = branch.CreatedBy;

        clock.UtcNow = clock.UtcNow.AddHours(1);
        currentUser.UserId = secondUserId;

        context.Entry(branch)
            .Property(x => x.Name)
            .CurrentValue = "New Name";

        context.SaveChanges();

        Assert.Equal(originalCreatedAt, branch.CreatedAtUtc);
        Assert.Equal(originalCreatedBy, branch.CreatedBy);

        Assert.Equal(clock.UtcNow, branch.UpdatedAtUtc);
        Assert.Equal(secondUserId.ToString(), branch.UpdatedBy);
    }

    [Fact]
    public void SaveChanges_WhenDeletingBranch_ShouldSoftDeleteIt()
    {
        var userId = Guid.NewGuid();

        var clock = new TestClock(
            new DateTime(2026, 11, 12, 16, 0, 0, DateTimeKind.Utc));

        var currentUser = new TestCurrentUser(userId);

        using var context = CreateContext(clock, currentUser);

        var branch = new Branch("BR-003", "Deleted Branch", "Cairo");

        context.Branches.Add(branch);
        context.SaveChanges();

        clock.UtcNow = clock.UtcNow.AddHours(1);

        context.Branches.Remove(branch);
        context.SaveChanges();

        Assert.True(branch.IsDeleted);
        Assert.Equal(clock.UtcNow, branch.DeletedAtUtc);
        Assert.Equal(userId.ToString(), branch.DeletedBy);

        Assert.False(context.Branches.Any(x => x.Id == branch.Id));

        Assert.True(
            context.Branches
                .IgnoreQueryFilters()
                .Any(x => x.Id == branch.Id));
    }
[Fact]
public async Task SaveChangesAsync_WhenAddingBranch_ShouldSetCreationAudit()
{
    var userId = Guid.NewGuid();

    var clock = new TestClock(
        new DateTime(2026, 11, 12, 16, 0, 0, DateTimeKind.Utc));

    var currentUser = new TestCurrentUser(userId);

    await using var context = CreateContext(clock, currentUser);

    var branch = new Branch("BR-004", "Async Branch", "Cairo");

    context.Branches.Add(branch);

    await context.SaveChangesAsync();

    Assert.Equal(clock.UtcNow, branch.CreatedAtUtc);
    Assert.Equal(userId.ToString(), branch.CreatedBy);
    Assert.Null(branch.UpdatedAtUtc);
    Assert.Null(branch.UpdatedBy);
}

    private static StoreXDbContext CreateContext(
        TestClock clock,
        TestCurrentUser currentUser)
    {
        var options = new DbContextOptionsBuilder<StoreXDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreXDbContext(options, clock, currentUser);
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow { get; set; }

        public TestClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }

        public Guid? BranchId => null;

        public TestCurrentUser(Guid? userId)
        {
            UserId = userId;
        }
    }
}
