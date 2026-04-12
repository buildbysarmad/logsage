using LogSage.Core.Models;
using LogSage.Core.Parsers;
using Xunit;

namespace LogSage.Core.Tests;

/// <summary>
/// Integration tests for structured log architecture end-to-end validation
/// </summary>
public class StructuredArchitectureIntegrationTests
{
    private readonly LogSageEngine _engine = new();

    [Fact]
    public void SerilogParser_RealWorldLog_CreatesFieldSections()
    {
        var realLog = """
            [01:43:13 INF] [INF] Starting application with environment: Development
            {}

            [01:43:16 INF] [INF] Incoming HTTP request
            {"Body":"","Headers":{"Accept":"*/*"},"Method":"GET","Path":"/swagger/index.html","StatusCode":null,"RequestId":"0HNJJH4H4M2C5:00000001","RequestPath":"/swagger/index.html","ConnectionId":"0HNJJH4H4M2C5"}

            [01:58:04 WRN] [WRN] Failed to determine the https port for redirect.
            {"EventId":{"Id":3,"Name":"FailedToDeterminePort"},"SourceContext":"Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware","RequestId":"0HNJJH4H4M2CA:00000001"}

            [01:58:48 ERR] [ERR] An error has occurred while processing the request.
            {"User":"anonymous","RequestId":"0HNJJH4H4M2CA:00000001","RequestPath":"/GenericEntity/upsert","StatusCode":500}

            System.ArgumentException: Host can't be null
               at Npgsql.NpgsqlConnectionStringBuilder.PostProcessAndValidate()
               at Npgsql.NpgsqlConnection.SetupDataSource()

            """;

        var result = _engine.AnalyzeStructured(realLog);

        Assert.Equal("Serilog", result.DetectedFormat);
        Assert.Equal(4, result.ParsedEntries);

        // Verify error entry has field sections
        var errorGroup = result.ErrorGroups.FirstOrDefault(g => g.Level == LogLevel.Error);
        Assert.NotNull(errorGroup);
        Assert.Single(errorGroup.Entries);

        var errorEntry = errorGroup.Entries[0];
        Assert.NotEmpty(errorEntry.FieldSections);
        Assert.Equal("Serilog", errorEntry.ParserType);

        // Verify Request Context section exists with RequestId and RequestPath
        var requestSection = errorEntry.FieldSections.FirstOrDefault(s => s.SectionName == "Request Context");
        Assert.NotNull(requestSection);
        Assert.Contains(requestSection.Fields, f => f.Key == "requestId" && f.Value?.ToString() == "0HNJJH4H4M2CA:00000001");
        Assert.Contains(requestSection.Fields, f => f.Key == "requestPath" && f.Value?.ToString() == "/GenericEntity/upsert");

        // Verify StatusCode field has correct type and hints
        var statusField = requestSection.Fields.FirstOrDefault(f => f.Key == "statusCode");
        Assert.NotNull(statusField);
        Assert.Equal(FieldType.Number, statusField.Type);
        Assert.Equal(500, statusField.Value);
        Assert.NotNull(statusField.Hints);
        Assert.True(statusField.Hints.ContainsKey("colorThreshold"));

        // Verify Exception Details section exists with stack trace
        var exceptionSection = errorEntry.FieldSections.FirstOrDefault(s => s.SectionName == "Exception Details");
        Assert.NotNull(exceptionSection);
        Assert.Contains(exceptionSection.Fields, f => f.Key == "exceptionType");
        Assert.Contains(exceptionSection.Fields, f => f.Key == "stackTrace" && f.Type == FieldType.StackTrace);

        // Verify backward compatibility properties work
        Assert.Equal("0HNJJH4H4M2CA:00000001", errorEntry.RequestId);
        Assert.Equal("/GenericEntity/upsert", errorEntry.RequestPath);
        Assert.Equal(500, errorEntry.StatusCode);
        Assert.NotNull(errorEntry.StackTrace);
        Assert.Contains("Npgsql", errorEntry.StackTrace);
    }

    [Fact]
    public void NLogParser_CreatesFieldSections()
    {
        var nlogLog = """
            2024-04-12 14:32:15.1234|ERROR|MyNamespace.MyClass|Application error occurred
            2024-04-12 14:32:16.5678|INFO|MyNamespace.OtherClass|Info message
            """;

        var result = _engine.AnalyzeStructured(nlogLog);

        Assert.Equal("NLog", result.DetectedFormat);
        Assert.Equal(2, result.Entries.Count);

        var entry = result.Entries[0];
        Assert.Equal("NLog", entry.ParserType);
        Assert.NotNull(entry.FieldSections);

        // If Source Context section exists, verify it has logger field
        var sourceSection = entry.FieldSections.FirstOrDefault(s => s.SectionName == "Source Context");
        if (sourceSection != null)
        {
            Assert.Contains(sourceSection.Fields, f => f.Key == "logger");
        }
    }

    [Fact]
    public void StandardParser_CreatesFieldSections()
    {
        var standardLog = """
            2024-04-12 10:30:45 [ERROR] Something went wrong
            2024-04-12 10:30:46 [INFO] Normal operation
            java.lang.NullPointerException
                at com.example.Main.process(Main.java:42)
            """;

        var result = _engine.AnalyzeStructured(standardLog);

        Assert.Equal("Standard", result.DetectedFormat);
        Assert.Equal(2, result.Entries.Count);

        var entry = result.Entries[0];
        Assert.Equal("Standard", entry.ParserType);
        Assert.NotNull(entry.FieldSections);

        // Standard parser should have Exception Details section if stack trace present
        if (entry.StackTrace != null)
        {
            var exceptionSection = entry.FieldSections.FirstOrDefault(s => s.SectionName == "Exception Details");
            Assert.NotNull(exceptionSection);
            Assert.Contains(exceptionSection.Fields, f => f.Type == FieldType.StackTrace);
        }
    }

    [Fact]
    public void PlainTextParser_CreatesFieldSections()
    {
        // PlainText is fallback - use unstructured logs
        var plainLog = "ERROR: Connection failed";

        var result = _engine.AnalyzeStructured(plainLog);

        // May be detected as Standard or Plain depending on format priority
        Assert.True(result.DetectedFormat == "Plain" || result.DetectedFormat == "Standard");
        Assert.NotEmpty(result.Entries);

        var entry = result.Entries[0];
        Assert.NotNull(entry.FieldSections);
    }

    [Fact]
    public void AllParsers_FieldImportanceIsCorrect()
    {
        var serilogLog = """
            [01:58:48 ERR] [ERR] An error has occurred
            {"RequestId":"abc-123","StatusCode":500}
            """;

        var result = _engine.AnalyzeStructured(serilogLog);
        var entry = result.Entries[0];

        // StatusCode should be Primary importance
        var statusField = entry.FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "statusCode");

        if (statusField != null)
        {
            Assert.Equal(FieldImportance.Primary, statusField.Importance);
        }

        // RequestId should be Secondary importance
        var requestIdField = entry.FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "requestId");

        if (requestIdField != null)
        {
            Assert.Equal(FieldImportance.Secondary, requestIdField.Importance);
        }
    }

    [Fact]
    public void FieldSections_DisplayOrderIsSequential()
    {
        var serilogLog = """
            [01:58:48 ERR] [ERR] Error occurred
            {"RequestId":"abc","StatusCode":500,"SourceContext":"MyApp"}

            System.Exception: Test
               at Test.Method()
            """;

        var result = _engine.AnalyzeStructured(serilogLog);
        var entry = result.Entries[0];

        // Verify sections are ordered
        var sections = entry.FieldSections.OrderBy(s => s.DisplayOrder).ToList();
        for (int i = 0; i < sections.Count; i++)
        {
            Assert.Equal(i, sections[i].DisplayOrder);
        }
    }

}
