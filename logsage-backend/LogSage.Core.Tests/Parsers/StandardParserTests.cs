using LogSage.Core.Models;
using LogSage.Core.Parsers;
using Xunit;

namespace LogSage.Core.Tests.Parsers;

public class StandardParserTests
{
    private readonly StandardFormatParser _parser = new();

    private const string SampleLog =
        "2024-03-14 02:14:33 [ERROR] NullReferenceException: Object reference not set\n" +
        "2024-03-14 02:14:34 [WARN] Slow query detected: 5200ms\n" +
        "2024-03-14 02:14:35 [INFO] Application started";

    [Fact]
    public void CanParse_WithValidStandardFormat_ReturnsTrue()
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
    public void Parse_ExtractsCorrectLevels()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal(LogLevel.Error, entries[0].Level);
        Assert.Equal(LogLevel.Warning, entries[1].Level);
        Assert.Equal(LogLevel.Info, entries[2].Level);
    }

    [Fact]
    public void Parse_ExtractsTimestamp()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.NotNull(entries[0].Timestamp);
        Assert.Equal(2024, entries[0].Timestamp!.Value.Year);
    }

    [Fact]
    public void Parse_ExtractsExceptionType()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal("NullReferenceException", entries[0].ExceptionType);
    }
}
