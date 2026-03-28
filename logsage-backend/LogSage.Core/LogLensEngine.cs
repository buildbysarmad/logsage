using LogSage.Core.Models;

namespace LogSage.Core;

public class ParseResult
{
    public string DetectedFormat { get; set; } = string.Empty;
    public int TotalLines { get; set; }
    public int ParsedEntries { get; set; }
    public List<LogEntry> Entries { get; set; } = new();
    public List<ErrorGroup> ErrorGroups { get; set; } = new();
    public int ErrorCount => Entries.Count(e => e.Level is LogLevel.Error or LogLevel.Fatal);
    public int WarningCount => Entries.Count(e => e.Level == LogLevel.Warning);
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

    public ParseResult Analyze(string rawLog)
    {
        if (string.IsNullOrWhiteSpace(rawLog)) return new ParseResult();

        var parser = _detector.Detect(rawLog);
        var entries = parser.Parse(rawLog).ToList();
        var groups = _grouper.Group(entries);

        return new ParseResult
        {
            DetectedFormat = parser.FormatName,
            TotalLines = rawLog.Split('\n').Length,
            ParsedEntries = entries.Count,
            Entries = entries,
            ErrorGroups = groups
        };
    }

    public async Task<ParseResult> AnalyzeFileAsync(string filePath)
    {
        var content = await File.ReadAllTextAsync(filePath);
        return Analyze(content);
    }

    public async Task<ParseResult> AnalyzeStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        return Analyze(content);
    }
}
