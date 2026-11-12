
using Microsoft.EntityFrameworkCore;
using StoreX.Api.Infrastructure.Persistence;
using StoreX.Api.Infrastructure.Persistence.Auditing;
using StoreX.Api.Modules.Organization.Entities;
using StoreX.Api.Shared;

namespace StoreX.Tests;

public class StoreXDbContextAuditTests
{
    [Fact]
    public void SaveChanges_WhenAddingBranch_ShouldSetCreationAuditAndCreateAuditLog()
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

        var auditLog = Assert.Single(context.AuditLogs);

        Assert.Equal(nameof(Branch), auditLog.EntityName);
        Assert.Equal(branch.Id, auditLog.EntityId);
        Assert.Equal("Added", auditLog.Action);
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal(clock.UtcNow, auditLog.ChangedAtUtc);
        Assert.Contains("Main Branch", auditLog.NewValues);
        Assert.Null(auditLog.OldValues);
    }

    [Fact]
    public void SaveChanges_WhenUpdatingBranch_ShouldSetUpdateAuditAndRecordChanges()
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

        var auditLogs = context.AuditLogs
            .OrderBy(x => x.ChangedAtUtc)
            .ToList();

        Assert.Equal(2, auditLogs.Count);

        var updateLog = Assert.Single(
            auditLogs,
            x => x.Action == "Modified");

        Assert.Equal(branch.Id, updateLog.EntityId);
        Assert.Equal(secondUserId, updateLog.UserId);
        Assert.Contains("Old Name", updateLog.OldValues);
        Assert.Contains("New Name", updateLog.NewValues);
    }

    [Fact]
    public void SaveChanges_WhenDeletingBranch_ShouldSoftDeleteAndCreateAuditLog()
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

        var auditLogs = context.AuditLogs.ToList();

        Assert.Equal(2, auditLogs.Count);

        var deleteLog = Assert.Single(
            auditLogs,
            x => x.Action == "Deleted");

        Assert.Equal(branch.Id, deleteLog.EntityId);
        Assert.Equal(userId, deleteLog.UserId);
        Assert.Contains("IsDeleted", deleteLog.NewValues);
        Assert.Contains("DeletedAtUtc", deleteLog.NewValues);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAddingBranch_ShouldSetCreationAuditAndCreateAuditLog()
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

        var auditLog = Assert.Single(context.AuditLogs);

        Assert.Equal("Added", auditLog.Action);
        Assert.Equal(branch.Id, auditLog.EntityId);
        Assert.Equal(userId, auditLog.UserId);
    }

    private static StoreXDbContext CreateContext(
        TestClock clock,
        TestCurrentUser currentUser)
    {
        var interceptor = new AuditSaveChangesInterceptor(
            clock,
            currentUser);

        var options = new DbContextOptionsBuilder<StoreXDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new StoreXDbContext(options);
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
