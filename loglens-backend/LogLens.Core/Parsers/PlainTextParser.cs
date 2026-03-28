using System.Text.RegularExpressions;
using LogLens.Core.Models;

namespace LogLens.Core.Parsers;

// Fallback: handles "ERROR: message" or any line with a level keyword
public class PlainTextParser : BaseLogParser
{
    public override string FormatName => "Plain";

    private static readonly Regex LevelRegex = new(
        @"\b(?<lvl>TRACE|DEBUG|INFO|INFORMATION|WARN|WARNING|ERROR|FATAL|CRITICAL)\b[\s:\-]*(?<msg>.+)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TimestampRegex = new(
        @"\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}", RegexOptions.Compiled);

    public override bool CanParse(string sampleLines) =>
        sampleLines.Split('\n').Any(l => LevelRegex.IsMatch(l));

    public override IEnumerable<LogEntry> Parse(string rawLog)
    {
        var lines = rawLog.Split('\n');
        var lineNum = 0;

        foreach (var line in lines)
        {
            lineNum++;
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            var levelMatch = LevelRegex.Match(trimmed);
            if (!levelMatch.Success) continue;

            var tsMatch = TimestampRegex.Match(trimmed);

            yield return new LogEntry
            {
                Timestamp = tsMatch.Success && DateTime.TryParse(tsMatch.Value, out var ts) ? ts : null,
                Level = ParseLevel(levelMatch.Groups["lvl"].Value),
                Message = levelMatch.Groups["msg"].Value.Trim(),
                ExceptionType = ExtractExceptionType(trimmed),
                LineNumber = lineNum,
                RawLine = trimmed
            };
        }
    }
}
