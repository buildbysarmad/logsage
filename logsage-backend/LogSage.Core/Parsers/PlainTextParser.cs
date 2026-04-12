using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// Fallback: handles "ERROR: message" or any line with a level keyword
public class PlainTextParser : BaseStructuredLogParser, ILogParser
{
    public override string FormatName => "Plain";

    private static readonly Regex LevelRegex = new(
        @"\b(?<lvl>TRACE|DEBUG|INFO|INFORMATION|WARN|WARNING|ERROR|FATAL|CRITICAL)\b[\s:\-]*(?<msg>.+)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TimestampRegex = new(
        @"\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}", RegexOptions.Compiled);

    public override bool CanParse(string sampleLines) =>
        sampleLines.Split('\n').Any(l => LevelRegex.IsMatch(l));

    public IEnumerable<LogEntry> Parse(string rawLog)
    {
        foreach (var structured in ParseStructured(rawLog))
        {
            yield return new LogEntry
            {
                Timestamp = structured.Timestamp,
                Level = structured.Level,
                Message = structured.Message,
                LineNumber = structured.LineNumber,
                RawLine = structured.RawLine,
                ExceptionType = structured.ExceptionType
            };
        }
    }

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
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
            var exceptionType = ExtractExceptionType(trimmed);

            var structured = new StructuredLogEntry
            {
                Timestamp = tsMatch.Success && DateTime.TryParse(tsMatch.Value, out var ts) ? ts : null,
                Level = ParseLevel(levelMatch.Groups["lvl"].Value),
                Message = levelMatch.Groups["msg"].Value.Trim(),
                LineNumber = lineNum,
                RawLine = trimmed,
                ParserType = FormatName,
                FieldSections = []
            };

            var sections = new List<FieldSection>();
            var sectionOrder = 0;

            // Section 1: Exception Details (if detected)
            var exceptionSection = CreateExceptionSection(exceptionType, null, sectionOrder++);
            if (exceptionSection != null)
                sections.Add(exceptionSection);

            structured.FieldSections = sections;
            yield return structured;
        }
    }
}
