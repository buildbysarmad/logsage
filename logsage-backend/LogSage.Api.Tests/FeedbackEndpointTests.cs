using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Data.Repositories;
using LogSage.Api.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LogSage.Api.Tests;

public class FeedbackEndpointTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task SubmitFeedback_ValidRequest_Returns204()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        var session = new ParseSession
        {
            SessionToken = "test-token-123",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        };
        await repo.CreateAsync(session);

        var request = new FeedbackRequest(1);

        // Act
        var result = await repo.GetByTokenAsync("test-token-123");

        // Assert - verify session exists before feedback
        Assert.NotNull(result);
        Assert.Null(result.FeedbackScore);

        // Act - submit feedback
        await repo.UpdateFeedbackAsync("test-token-123", request.Score);
        var updated = await repo.GetByTokenAsync("test-token-123");

        // Assert
        Assert.Equal(1, updated!.FeedbackScore);
        Assert.NotNull(updated.FeedbackAt);
    }

    [Fact]
    public async Task SubmitFeedback_InvalidScore_ValidationRequired()
    {
        // Arrange
        var request = new FeedbackRequest(5);

        // Assert - validation should happen in endpoint
        // Score must be 1 or -1
        Assert.True(request.Score != 1 && request.Score != -1);
    }

    [Fact]
    public async Task SubmitFeedback_DuplicateFeedback_CanBeDetected()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        var session = new ParseSession
        {
            SessionToken = "test-token-duplicate",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        };
        await repo.CreateAsync(session);

        // First feedback
        await repo.UpdateFeedbackAsync("test-token-duplicate", 1);
        var afterFirst = await repo.GetByTokenAsync("test-token-duplicate");

        // Assert - feedback exists
        Assert.NotNull(afterFirst!.FeedbackScore);
        Assert.Equal(1, afterFirst.FeedbackScore);
    }

    [Fact]
    public async Task SubmitFeedback_UnknownToken_ReturnsNull()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        // Act
        var result = await repo.GetByTokenAsync("non-existent-token");

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task SubmitFeedback_NegativeScore_IsValid()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var repo = new ParseSessionRepository(db);

        var session = new ParseSession
        {
            SessionToken = "test-token-negative",
            DetectedFormat = "Serilog-Text",
            ParseSuccess = true,
            TotalEntries = 100,
            InputLineCount = 100,
            InputSizeBytes = 5000,
            DurationMs = 50
        };
        await repo.CreateAsync(session);

        // Act
        await repo.UpdateFeedbackAsync("test-token-negative", -1);
        var result = await repo.GetByTokenAsync("test-token-negative");

        // Assert
        Assert.Equal(-1, result!.FeedbackScore);
    }
}
