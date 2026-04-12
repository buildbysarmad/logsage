using LogSage.Core.Models;
using LogSage.Core.Parsers;
using Xunit;

namespace LogSage.Core.Tests.Parsers;

public class SerilogParserTests
{
    private readonly SerilogFormatParser _parser = new();

    [Fact]
    public void CanParse_WithRealWorldSerilogFormat_ReturnsTrue()
    {
        const string sample = """
            [01:43:13 INF] [INF] Starting application
            {}

            [01:43:16 INF] [INF] Request received
            {"RequestId":"123"}

            """;
        Assert.True(_parser.CanParse(sample));
    }

    [Fact]
    public void CanParse_WithFullTimestampFormat_ReturnsTrue()
    {
        const string sample = "2024-03-14 02:14:33.123 +05:00 [ERR] SqlException: Connection timeout\n   at UserService.GetProfile()\n\n2024-03-14 02:14:34.456 +05:00 [INF] Request completed";
        Assert.True(_parser.CanParse(sample));
    }

    [Fact]
    public void Parse_NormalINFEntry_ParsesCorrectly()
    {
        const string log = """
            [01:43:13 INF] [INF] Starting application with environment: Development
            {}

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        Assert.Equal(LogLevel.Info, entries[0].Level);
        Assert.Equal("Starting application with environment: Development", entries[0].Message);
        Assert.NotNull(entries[0].Timestamp);
        Assert.Empty(entries[0].StructuredFields);
        Assert.Null(entries[0].StackTrace);
    }

    [Fact]
    public void Parse_HTTPRequestEntry_ParsesStructuredFields()
    {
        const string log = """
            [01:43:16 INF] [INF] Incoming HTTP request
            {"Body":"","Headers":{"Accept":"*/*"},"Method":"GET","Path":"/swagger/index.html","StatusCode":null,"RequestId":"0HNJJH4H4M2C5:00000001","RequestPath":"/swagger/index.html","ConnectionId":"0HNJJH4H4M2C5"}

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        var entry = entries[0];
        Assert.Equal(LogLevel.Info, entry.Level);
        Assert.Equal("Incoming HTTP request", entry.Message);

        // Check structured fields
        Assert.Equal("0HNJJH4H4M2C5:00000001", entry.RequestId);
        Assert.Equal("/swagger/index.html", entry.RequestPath);
        Assert.Equal("0HNJJH4H4M2C5", entry.ConnectionId);

        Assert.True(entry.StructuredFields.ContainsKey("Method"));
        Assert.Equal("GET", entry.StructuredFields["Method"].ToString());
    }

    [Fact]
    public void Parse_ERREntryWithStackTrace_ParsesCorrectly()
    {
        const string log = """
            [01:58:48 ERR] [ERR] An error has occurred while processing the request.
            {"User":"anonymous","RequestId":"0HNJJH4H4M2CA:00000001","RequestPath":"/GenericEntity/upsert"}

            System.ArgumentException: Host can't be null
               at Npgsql.NpgsqlConnectionStringBuilder.PostProcessAndValidate()
               at Npgsql.NpgsqlConnection.SetupDataSource()
               at Npgsql.NpgsqlConnection.set_ConnectionString(String value)

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        var entry = entries[0];
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("An error has occurred while processing the request.", entry.Message);

        // Check structured fields
        Assert.Equal("anonymous", entry.StructuredFields["User"].ToString());
        Assert.Equal("0HNJJH4H4M2CA:00000001", entry.RequestId);

        // Check stack trace
        Assert.NotNull(entry.StackTrace);
        Assert.Contains("System.ArgumentException: Host can't be null", entry.StackTrace);
        Assert.Contains("Npgsql.NpgsqlConnectionStringBuilder.PostProcessAndValidate", entry.StackTrace);

        // Check exception type
        Assert.Equal("ArgumentException", entry.ExceptionType);
    }

    [Fact]
    public void Parse_WRNEntryWithEventId_ParsesCorrectly()
    {
        const string log = """
            [01:58:04 WRN] [WRN] Failed to determine the https port for redirect.
            {"EventId":{"Id":3,"Name":"FailedToDeterminePort"},"SourceContext":"Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware","RequestId":"0HNJJH4H4M2CA:00000001"}

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        var entry = entries[0];
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("Failed to determine the https port for redirect.", entry.Message);

        // Check source context
        Assert.Equal("Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware", entry.SourceContext);
        Assert.Equal("Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware", entry.Source);
    }

    [Fact]
    public void Parse_MultipleConsecutiveEntries_ParsesAll()
    {
        const string log = """
            [01:43:13 INF] [INF] Starting application
            {}

            [01:43:16 INF] [INF] Request received
            {"RequestId":"123"}

            [01:58:04 WRN] [WRN] Warning message
            {}

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Equal(3, entries.Count);
        Assert.Equal(LogLevel.Info, entries[0].Level);
        Assert.Equal(LogLevel.Info, entries[1].Level);
        Assert.Equal(LogLevel.Warning, entries[2].Level);
    }

    [Fact]
    public void Parse_EntryWithCRLFSeparators_ParsesCorrectly()
    {
        const string log = "[01:43:13 INF] [INF] Starting application\r\n{}\r\n\r\n";

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        Assert.Equal(LogLevel.Info, entries[0].Level);
    }

    [Fact]
    public void Parse_InvalidJSONPayload_MarksParseError()
    {
        const string log = """
            [01:43:13 INF] [INF] Test message
            {invalid json}

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        Assert.True(entries[0].ParseError);
        Assert.NotNull(entries[0].ParseErrorMessage);
        Assert.Contains("JSON parse error", entries[0].ParseErrorMessage);
    }

    [Fact]
    public void Parse_AllLogLevels_ParsesCorrectly()
    {
        const string log = """
            [01:00:00 VRB] [VRB] Verbose message
            {}

            [02:00:00 DBG] [DBG] Debug message
            {}

            [03:00:00 INF] [INF] Info message
            {}

            [04:00:00 WRN] [WRN] Warning message
            {}

            [05:00:00 ERR] [ERR] Error message
            {}

            [06:00:00 FTL] [FTL] Fatal message
            {}

            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Equal(6, entries.Count);
        Assert.Equal(LogLevel.Trace, entries[0].Level);
        Assert.Equal(LogLevel.Debug, entries[1].Level);
        Assert.Equal(LogLevel.Info, entries[2].Level);
        Assert.Equal(LogLevel.Warning, entries[3].Level);
        Assert.Equal(LogLevel.Error, entries[4].Level);
        Assert.Equal(LogLevel.Fatal, entries[5].Level);
    }

    [Fact]
    public void Parse_FullTimestampFormat_ParsesCorrectly()
    {
        const string log = """
            2024-03-14 02:14:33.123 +05:00 [ERR] SqlException: Connection timeout
               at UserService.GetProfile()
            """;

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        var entry = entries[0];
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.NotNull(entry.Timestamp);
        Assert.Equal(2024, entry.Timestamp.Value.Year);
        Assert.Equal(3, entry.Timestamp.Value.Month);
        Assert.Equal(14, entry.Timestamp.Value.Day);
    }

    [Fact]
    public void Parse_EntryWithLargeBody_DoesNotCrash()
    {
        var largeBody = new string('x', 50000);
        var log = "[01:43:16 INF] [INF] Large request\n" +
                  $"{{\"Body\":\"{largeBody}\",\"RequestId\":\"123\"}}\n\n";

        var entries = _parser.Parse(log).ToList();

        Assert.Single(entries);
        Assert.Equal("123", entries[0].RequestId);
    }
}
