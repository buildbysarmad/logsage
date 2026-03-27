using LogLens.Core.Models;
using Xunit;

namespace LogLens.Core.Tests;

public class ErrorGrouperTests
{
    private readonly ErrorGrouper _grouper = new();

    [Fact]
    public void Group_SimilarExceptions_GroupedTogether()
    {
        var entries = new List<LogEntry>
        {
            new() { Level = LogLevel.Error, Message = "SqlException: timeout for userId=1", ExceptionType = "SqlException" },
            new() { Level = LogLevel.Error, Message = "SqlException: timeout for userId=2", ExceptionType = "SqlException" },
            new() { Level = LogLevel.Error, Message = "SqlException: timeout for userId=3", ExceptionType = "SqlException" },
        };

        var groups = _grouper.Group(entries);

        Assert.Single(groups);
        Assert.Equal(3, groups[0].Count);
        Assert.Equal("SqlException", groups[0].ExceptionType);
    }

    [Fact]
    public void Group_DifferentExceptions_GroupedSeparately()
    {
        var entries = new List<LogEntry>
        {
            new() { Level = LogLevel.Error, Message = "SqlException: timeout", ExceptionType = "SqlException" },
            new() { Level = LogLevel.Error, Message = "NullReferenceException: object not set", ExceptionType = "NullReferenceException" },
        };

        var groups = _grouper.Group(entries);

        Assert.Equal(2, groups.Count);
    }

    [Fact]
    public void Group_InfoEntries_NotIncluded()
    {
        var entries = new List<LogEntry>
        {
            new() { Level = LogLevel.Info, Message = "Request completed" },
            new() { Level = LogLevel.Error, Message = "SqlException: timeout", ExceptionType = "SqlException" },
        };

        var groups = _grouper.Group(entries);

        Assert.Single(groups);
    }

    [Fact]
    public void Group_TracksFirstAndLastSeen()
    {
        var now = DateTime.UtcNow;
        var entries = new List<LogEntry>
        {
            new() { Level = LogLevel.Error, ExceptionType = "SqlException", Message = "timeout", Timestamp = now.AddMinutes(-10) },
            new() { Level = LogLevel.Error, ExceptionType = "SqlException", Message = "timeout", Timestamp = now },
        };

        var groups = _grouper.Group(entries);

        Assert.Equal(now.AddMinutes(-10), groups[0].FirstSeen);
        Assert.Equal(now, groups[0].LastSeen);
    }
}
