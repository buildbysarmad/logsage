using LogSage.Core.Models;
using LogSage.Core.Parsers;

namespace LogSage.Core;

[Obsolete("Use StructuredParseResult instead. This class will be removed in a future version.")]
public class ParseResult
{
    public string DetectedFormat { get; set; } = string.Empty;
    public int TotalLines { get; set; }
    public int ParsedEntries { get; set; }
    public List<LogEntry> Entries { get; set; } = new();
    public List<ErrorGroup> ErrorGroups { get; set; } = new();
    public int ErrorCount => Entries.Count(e => e.Level is LogLevel.Error or LogLevel.Fatal);
    public int WarningCount => Entries.Count(e => e.Level == LogLevel.Warning);
    public int DebugCount => Entries.Count(e => e.Level == LogLevel.Debug);
    public int InfoCount => Entries.Count(e => e.Level == LogLevel.Info);
    public int ParseErrorCount => Entries.Count(e => e.ParseError);
    public List<string> UnparsedLines { get; set; } = new();
    public TimeSpan? LogDuration =>
        Entries.Any(e => e.Timestamp.HasValue)
            ? Entries.Where(e => e.Timestamp.HasValue).Max(e => e.Timestamp) -
              Entries.Where(e => e.Timestamp.HasValue).Min(e => e.Timestamp)
            : null;
}

public class StructuredParseResult
{
    public string DetectedFormat { get; set; } = string.Empty;
    public int TotalLines { get; set; }
    public int ParsedEntries { get; set; }
    public List<StructuredLogEntry> Entries { get; set; } = [];
    public List<ErrorGroup> ErrorGroups { get; set; } = [];
    public int ErrorCount => Entries.Count(e => e.Level is LogLevel.Error or LogLevel.Fatal);
    public int WarningCount => Entries.Count(e => e.Level == LogLevel.Warning);
    public int DebugCount => Entries.Count(e => e.Level == LogLevel.Debug);
    public int InfoCount => Entries.Count(e => e.Level == LogLevel.Info);
    public int ParseErrorCount => Entries.Count(e => e.ParseError);
    public List<string> UnparsedLines { get; set; } = [];
    public TimeSpan? LogDuration =>
        Entries.Any(e => e.Timestamp.HasValue)
            ? Entries.Where(e => e.Timestamp.HasValue).Max(e => e.Timestamp) -
              Entries.Where(e => e.Timestamp.HasValue).Min(e => e.Timestamp)
            : null;
}

public class LogSageEngine
{
    private readonly FormatDetector _detector = new();
    private readonly ErrorGrouper _grouper = new();

    [Obsolete("Use AnalyzeStructured() instead. This method will be removed in a future version.")]
    public ParseResult Analyze(string rawLog)
    {
        if (string.IsNullOrWhiteSpace(rawLog)) return new ParseResult();

        var parser = _detector.Detect(rawLog);
        var entries = parser.Parse(rawLog).ToList();
        var groups = _grouper.Group(entries.Select(e => e.ToStructured(parser.FormatName)).ToList());

        return new ParseResult
        {
            DetectedFormat = parser.FormatName,
            TotalLines = rawLog.Split('\n').Length,
            ParsedEntries = entries.Count,
            Entries = entries,
            ErrorGroups = groups
        };
    }

    public StructuredParseResult AnalyzeStructured(string rawLog)
    {
        if (string.IsNullOrWhiteSpace(rawLog)) return new StructuredParseResult();

        var parser = _detector.Detect(rawLog) as IStructuredLogParser;
        if (parser == null)
            throw new InvalidOperationException("Parser does not support structured parsing");

        var entries = parser.ParseStructured(rawLog).ToList();
        var groups = _grouper.Group(entries);

        return new StructuredParseResult
        {
            DetectedFormat = parser.FormatName,
            TotalLines = rawLog.Split('\n').Length,
            ParsedEntries = entries.Count,
            Entries = entries,
            ErrorGroups = groups
        };
    }

    public async Task<StructuredParseResult> AnalyzeFileAsync(string filePath)
    {
        var content = await File.ReadAllTextAsync(filePath);
        return AnalyzeStructured(content);
    }

    public async Task<StructuredParseResult> AnalyzeStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        return AnalyzeStructured(content);
    }
}
