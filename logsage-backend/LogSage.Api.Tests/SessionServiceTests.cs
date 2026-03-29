using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Tests;

public class SessionServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task SessionService_BlocksRequest_WhenDailyLimitExceeded()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var service = new SessionService(db);
        var identifier = "test-user-123";
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Add usage record at limit
        db.UsageTracking.Add(new UsageTracking
        {
            Identifier = identifier,
            Date = today,
            SessionCount = 3 // Free tier limit
        });
        await db.SaveChangesAsync();

        // Act
        var isWithinLimit = await service.IsWithinFreeTierLimitAsync(identifier);

        // Assert
        Assert.False(isWithinLimit); // Should be blocked
    }

    [Fact]
    public async Task SessionService_AllowsRequest_WhenUnderDailyLimit()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var service = new SessionService(db);
        var identifier = "test-user-456";
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Add usage record under limit
        db.UsageTracking.Add(new UsageTracking
        {
            Identifier = identifier,
            Date = today,
            SessionCount = 2 // Under limit of 3
        });
        await db.SaveChangesAsync();

        // Act
        var isWithinLimit = await service.IsWithinFreeTierLimitAsync(identifier);

        // Assert
        Assert.True(isWithinLimit); // Should be allowed
    }

    [Fact]
    public async Task SessionService_AllowsRequest_WhenNoUsageRecordExists()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var service = new SessionService(db);
        var identifier = "new-user-789";

        // Act
        var isWithinLimit = await service.IsWithinFreeTierLimitAsync(identifier);

        // Assert
        Assert.True(isWithinLimit); // New user should be allowed
    }

    [Fact(Skip = "InMemory database does not support ExecuteUpdateAsync used by IncrementUsageAsync")]
    public async Task SessionService_CreatesNewRecord_OnFirstIncrement()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var service = new SessionService(db);
        var identifier = "new-user-first-increment";

        // Act
        await service.IncrementUsageAsync(identifier);

        // Assert
        var usage = await db.UsageTracking.FirstOrDefaultAsync(u => u.Identifier == identifier);
        Assert.NotNull(usage);
        Assert.Equal(1, usage.SessionCount);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), usage.Date);
    }
}
