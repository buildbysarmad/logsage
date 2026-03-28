using LogLens.Core.Parsers;
using Xunit;

namespace LogLens.Core.Tests;

public class FormatDetectorTests
{
    private readonly FormatDetector _detector = new();

    [Fact]
    public void Detect_SerilogLog_ReturnsSerilogParser()
    {
        var log =
            "2024-03-14 02:14:33.123 +05:00 [ERR] Something failed\n" +
            "2024-03-14 02:14:34.456 +05:00 [INF] Request done";
        Assert.Equal("Serilog", _detector.DetectFormatName(log));
    }

    [Fact]
    public void Detect_NLogLog_ReturnsNLogParser()
    {
        var log =
            "2024-03-14 02:14:33.1234|ERROR|MyApp|Something failed\n" +
            "2024-03-14 02:14:34.5678|INFO|MyApp|Request done";
        Assert.Equal("NLog", _detector.DetectFormatName(log));
    }

    [Fact]
    public void Detect_StandardLog_ReturnsStandardParser()
    {
        var log =
            "2024-03-14 02:14:33 [ERROR] Something failed\n" +
            "2024-03-14 02:14:34 [INFO] Request done";
        Assert.Equal("Standard", _detector.DetectFormatName(log));
    }

    [Fact]
    public void Detect_PlainTextLog_ReturnsPlainParser()
    {
        // Level keyword mid-line — won't match StandardFormatParser (requires level at start)
        var log = "Application encountered an ERROR condition\nSystem WARN: low disk space detected";
        Assert.Equal("Plain", _detector.DetectFormatName(log));
    }
}
