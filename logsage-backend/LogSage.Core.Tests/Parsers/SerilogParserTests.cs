using LogSage.Core.Models;
using LogSage.Core.Parsers;
using Xunit;

namespace LogSage.Core.Tests.Parsers;

public class SerilogParserTests
{
    private readonly SerilogFormatParser _parser = new();

    private const string SampleLog = """
        2024-03-14 02:14:33.123 +05:00 [ERR] SqlException: Connection timeout expired
           at UserService.GetProfile() in UserService.cs:line 42
        2024-03-14 02:14:34.456 +05:00 [WRN] High memory usage detected: 89%
        2024-03-14 02:14:35.789 +05:00 [INF] Request completed in 234ms
        """;

    [Fact]
    public void CanParse_WithValidSerilogFormat_ReturnsTrue()
    {
        Assert.True(_parser.CanParse(SampleLog));
    }

    [Fact]
    public void Parse_ExtractsCorrectEntryCount()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal(3, entries.Count);
    }

    [Fact]
    public void Parse_ExtractsCorrectLevel()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal(LogLevel.Error, entries[0].Level);
        Assert.Equal(LogLevel.Warning, entries[1].Level);
        Assert.Equal(LogLevel.Info, entries[2].Level);
    }

    [Fact]
    public void Parse_ExtractsExceptionType()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal("SqlException", entries[0].ExceptionType);
    }

    [Fact]
    public void Parse_ExtractsStackTrace()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.NotNull(entries[0].StackTrace);
        Assert.Contains("UserService.GetProfile", entries[0].StackTrace);
    }
}
