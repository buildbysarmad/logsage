using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Services;
using LogSage.Core;
using LogSage.Core.Models;
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

    [Fact]
    public async Task SaveSessionAsync_SavesSessionWithCorrectCounts()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var service = new SessionService(db);
        var userId = Guid.NewGuid();

        // Create a user first
        db.Users.Add(new User { Id = userId, Email = "test@example.com" });
        await db.SaveChangesAsync();

        var parseResult = new ParseResult
        {
            DetectedFormat = "Serilog",
            TotalLines = 100,
            ParsedEntries = 95,
            Entries =
            [
                new LogEntry { Level = LogLevel.Error, Message = "Error 1", ParseError = false },
                new LogEntry { Level = LogLevel.Error, Message = "Error 2", ParseError = false },
                new LogEntry { Level = LogLevel.Fatal, Message = "Fatal 1", ParseError = false },
                new LogEntry { Level = LogLevel.Warning, Message = "Warning 1", ParseError = false },
                new LogEntry { Level = LogLevel.Warning, Message = "Warning 2", ParseError = false },
                new LogEntry { Level = LogLevel.Warning, Message = "Warning 3", ParseError = false },
                new LogEntry { Level = LogLevel.Info, Message = "Info 1", ParseError = false },
                new LogEntry { Level = LogLevel.Debug, Message = "Debug 1", ParseError = false }
            ],
            ErrorGroups =
            [
                new ErrorGroup
                {
                    GroupKey = "ERROR_1",
                    RepresentativeMessage = "Error message 1",
                    Level = LogLevel.Error,
                    Count = 2,
                    FirstSeen = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                },
                new ErrorGroup
                {
                    GroupKey = "WARNING_1",
                    RepresentativeMessage = "Warning message 1",
                    Level = LogLevel.Warning,
                    Count = 3,
                    FirstSeen = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                }
            ]
        };

        // Act
        var sessionId = await service.SaveSessionAsync(userId, parseResult);

        // Assert
        var savedSession = await db.Sessions
            .Include(s => s.ErrorGroups)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        Assert.NotNull(savedSession);
        Assert.Equal(userId, savedSession.UserId);
        Assert.Equal("Serilog", savedSession.DetectedFormat);
        Assert.Equal(100, savedSession.TotalLines);
        Assert.Equal(3, savedSession.ErrorCount); // 2 errors + 1 fatal
        Assert.Equal(3, savedSession.WarningCount); // 3 warnings
        Assert.Equal(2, savedSession.ErrorGroups.Count);
        Assert.Contains(savedSession.ErrorGroups, g => g.GroupKey == "ERROR_1");
        Assert.Contains(savedSession.ErrorGroups, g => g.GroupKey == "WARNING_1");
    }

    [Fact]
    public async Task SaveSessionAsync_SavesAnonymousSession_WhenUserIdIsNull()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var service = new SessionService(db);

        var parseResult = new ParseResult
        {
            DetectedFormat = "GenericLog",
            TotalLines = 50,
            ParsedEntries = 48,
            Entries =
            [
                new LogEntry { Level = LogLevel.Error, Message = "Error", ParseError = false }
            ],
            ErrorGroups = []
        };

        // Act
        var sessionId = await service.SaveSessionAsync(null, parseResult);

        // Assert
        var savedSession = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        Assert.NotNull(savedSession);
        Assert.Null(savedSession.UserId);
        Assert.Equal("GenericLog", savedSession.DetectedFormat);
        Assert.Equal(50, savedSession.TotalLines);
        Assert.Equal(1, savedSession.ErrorCount);
    }
}
