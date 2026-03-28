using LogLens.Core.Models;
using LogLens.Core.Parsers;
using Xunit;

namespace LogLens.Core.Tests.Parsers;

public class NLogParserTests
{
    private readonly NLogFormatParser _parser = new();

    private const string SampleLog =
        "2024-03-14 02:14:33.1234|ERROR|MyApp.Service|SqlException: connection refused\n" +
        "2024-03-14 02:14:34.5678|WARN|MyApp.Service|Retry attempt 3\n" +
        "2024-03-14 02:14:35.9012|INFO|MyApp.Web|Request completed";

    [Fact]
    public void CanParse_WithValidNLogFormat_ReturnsTrue()
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
    public void Parse_ExtractsSource()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal("MyApp.Service", entries[0].Source);
    }

    [Fact]
    public void Parse_ExtractsCorrectLevels()
    {
        var entries = _parser.Parse(SampleLog).ToList();
        Assert.Equal(LogLevel.Error, entries[0].Level);
        Assert.Equal(LogLevel.Warning, entries[1].Level);
        Assert.Equal(LogLevel.Info, entries[2].Level);
    }
}
