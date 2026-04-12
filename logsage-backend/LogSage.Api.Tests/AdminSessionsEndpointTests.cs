using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Tests;

public class AdminSessionsEndpointTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetSessions_ReturnsAllSessions_WhenNoFilter()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        // Create test sessions
        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "token1",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            ErrorCount = 0,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        });

        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "token2",
            DetectedFormat = "Serilog-JSON",
            ParseSuccess = false,
            TotalEntries = 50,
            ErrorCount = 10,
            InputLineCount = 50,
            InputSizeBytes = 2500,
            DurationMs = 30
        });

        // Act
        var filter = new ParseSessionFilter(null, null, null);
        var (items, total) = await repo.GetPagedAsync(filter, 1, 50);

        // Assert
        Assert.Equal(2, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetSessions_FiltersByParseSuccess()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "success-token",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        });

        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "failure-token",
            DetectedFormat = "Serilog-JSON",
            ParseSuccess = false,
            TotalEntries = 50,
            ParseErrorCount = 5,
            InputLineCount = 50,
            InputSizeBytes = 2500,
            DurationMs = 30
        });

        // Act
        var filter = new ParseSessionFilter(ParseSuccess: true, null, null);
        var (items, total) = await repo.GetPagedAsync(filter, 1, 50);

        // Assert
        Assert.Equal(1, total);
        Assert.Single(items);
        Assert.True(items[0].ParseSuccess);
    }

    [Fact]
    public async Task GetSessions_FiltersByHasErrors()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "no-errors",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            ErrorCount = 0,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        });

        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "has-errors",
            DetectedFormat = "Serilog-JSON",
            ParseSuccess = true,
            TotalEntries = 50,
            ErrorCount = 10,
            InputLineCount = 50,
            InputSizeBytes = 2500,
            DurationMs = 30
        });

        // Act
        var filter = new ParseSessionFilter(null, HasErrors: true, null);
        var (items, total) = await repo.GetPagedAsync(filter, 1, 50);

        // Assert
        Assert.Equal(1, total);
        Assert.Single(items);
        Assert.True(items[0].ErrorCount > 0);
    }

    [Fact]
    public async Task GetSessions_FiltersByHasFeedback()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        var sessionWithFeedback = new ParseSession
        {
            SessionToken = "with-feedback",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        };
        await repo.CreateAsync(sessionWithFeedback);
        await repo.UpdateFeedbackAsync("with-feedback", 1, "Great!");

        await repo.CreateAsync(new ParseSession
        {
            SessionToken = "no-feedback",
            DetectedFormat = "Serilog-JSON",
            ParseSuccess = true,
            TotalEntries = 50,
            InputLineCount = 50,
            InputSizeBytes = 2500,
            DurationMs = 30
        });

        // Act
        var filter = new ParseSessionFilter(null, null, HasFeedback: true);
        var (items, total) = await repo.GetPagedAsync(filter, 1, 50);

        // Assert
        Assert.Equal(1, total);
        Assert.Single(items);
        Assert.NotNull(items[0].FeedbackScore);
    }

    [Fact]
    public async Task GetSessions_PaginatesCorrectly()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        // Create 5 sessions
        for (var i = 1; i <= 5; i++)
        {
            await repo.CreateAsync(new ParseSession
            {
                SessionToken = $"token{i}",
                DetectedFormat = "Serilog-Text",
                ParseSuccess = true,
                TotalEntries = 100,
                InputLineCount = 100,
                InputSizeBytes = 5000,
                DurationMs = 50
            });
        }

        // Act - get page 1 with size 2
        var filter = new ParseSessionFilter(null, null, null);
        var (page1Items, total) = await repo.GetPagedAsync(filter, 1, 2);

        // Assert
        Assert.Equal(5, total);
        Assert.Equal(2, page1Items.Count);

        // Act - get page 2
        var (page2Items, _) = await repo.GetPagedAsync(filter, 2, 2);

        // Assert
        Assert.Equal(2, page2Items.Count);
    }

    [Fact]
    public async Task GetSessions_OrdersByCreatedAtDescending()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        var firstSession = new ParseSession
        {
            SessionToken = "first",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var secondSession = new ParseSession
        {
            SessionToken = "second",
            DetectedFormat = "Serilog-JSON",
            ParseSuccess = true,
            TotalEntries = 50,
            InputLineCount = 50,
            InputSizeBytes = 2500,
            DurationMs = 30,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        await repo.CreateAsync(firstSession);
        await repo.CreateAsync(secondSession);

        // Act
        var filter = new ParseSessionFilter(null, null, null);
        var (items, _) = await repo.GetPagedAsync(filter, 1, 50);

        // Assert - most recent first
        Assert.Equal(2, items.Count);
        Assert.True(items[0].CreatedAt > items[1].CreatedAt);
    }
}
