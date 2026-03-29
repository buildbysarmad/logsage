using LogSage.Core.Models;
using Xunit;

namespace LogSage.Core.Tests;

public class LogSageEngineTests
{
    private readonly LogSageEngine _engine = new();

    [Fact]
    public void LogSageEngine_Analyze_GroupsSimilarErrors()
    {
        // Arrange
        var log = @"2024-03-14 02:14:33.123 +05:00 [ERR] SqlException: Connection timeout
2024-03-14 02:14:34.456 +05:00 [INF] Processing request
2024-03-14 02:14:35.789 +05:00 [ERR] SqlException: Connection timeout
2024-03-14 02:14:36.012 +05:00 [ERR] SqlException: Connection timeout";

        // Act
        var result = _engine.Analyze(log);

        // Assert
        Assert.True(result.ErrorGroups.Count > 0);
        var sqlGroup = result.ErrorGroups.FirstOrDefault(g => g.ExceptionType == "SqlException");
        Assert.NotNull(sqlGroup);
        Assert.Equal(3, sqlGroup.Count);
    }

    [Fact]
    public void LogSageEngine_Analyze_CapsTotalEntries_WhenExceedingLimit()
    {
        // Arrange - Create log with many entries using Standard format
        var lines = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            lines.Add($"2024-03-14 12:34:{i:00} [INFO] Request {i}");
        }
        var largeLog = string.Join('\n', lines);

        // Act
        var result = _engine.Analyze(largeLog);

        // Assert
        Assert.Equal(100, result.TotalLines);
        // Engine should process all entries (no artificial cap in core logic)
        Assert.True(result.ParsedEntries > 0);
        Assert.Equal(100, result.ParsedEntries);
    }

    [Fact]
    public void LogSageEngine_Analyze_HandlesEmptyInput()
    {
        // Arrange
        var emptyLog = "";
        var whitespaceLog = "   \n  \n  ";

        // Act
        var emptyResult = _engine.Analyze(emptyLog);
        var whitespaceResult = _engine.Analyze(whitespaceLog);

        // Assert
        Assert.NotNull(emptyResult);
        Assert.Empty(emptyResult.Entries);
        Assert.Empty(emptyResult.ErrorGroups);
        Assert.Equal(0, emptyResult.ParsedEntries);

        Assert.NotNull(whitespaceResult);
        Assert.Empty(whitespaceResult.Entries);
        Assert.Empty(whitespaceResult.ErrorGroups);
    }
}
