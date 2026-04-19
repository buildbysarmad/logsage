using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

/// <summary>
/// Parses Serilog compact text formats including bracketed timestamps,
/// space-separated levels, timestamps without milliseconds, ISO8601 'T' format,
/// time-only timestamps, and custom separators.
/// </summary>
/// <remarks>
/// Handles formats:
/// - [2025-06-12 09:57:14.849 +07:00] [INF] [SourceContext] Message
/// - 2023-10-18 20:16:37.781 +03:00  INF  Message
/// - 2025-06-27 12:30:34 [INF] Message
/// - 2023-03-23T18:23:25.1406889+05:30 [INF] (Microsoft.Hosting.Lifetime) Message
/// - 22:43:04.863 +05:00 [Information] [Microsoft.Hosting.Lifetime] Message (time-only)
/// - [Warning] 28-06 02:35:34 || Message (custom separator)
/// </remarks>
public class SerilogCompactParser : BaseStructuredLogParser, ILogParser
{
    public override string FormatName => "Serilog";

    // Legacy interface for backward compatibility
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
                StackTrace = structured.StackTrace,
                ExceptionType = structured.ExceptionType
            };
        }
    }

    // Pattern 1: ISO8601 'T' format with optional source context in parentheses
    // 2023-03-23T18:23:25.1406889+05:30 [INF] (Microsoft.Hosting.Lifetime) Message
    private static readonly Regex Iso8601Regex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+[+-]\d{2}:\d{2})\s\[(?<lvl>VRB|DBG|INF|WRN|ERR|FTL)\](?:\s\([^\)]*\))?\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Pattern 2: Bracketed timestamp with optional source context
    // [2025-06-12 09:57:14.849 +07:00] [INF] [Microsoft.Hosting.Lifetime] [] Message
    private static readonly Regex BracketedRegex = new(
        @"^\[(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d+\s[+-]\d{2}:\d{2})\]\s\[(?<lvl>VRB|DBG|INF|WRN|ERR|FTL)\](?:\s\[[^\]]*\])*\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Pattern 3: Space-separated level (no brackets on level)
    // 2023-10-18 20:16:37.781 +03:00  INF  Message
    private static readonly Regex SpaceSeparatedRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d+\s[+-]\d{2}:\d{2})\s{2,}(?<lvl>VRB|DBG|INF|WRN|ERR|FTL)\s{2,}(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Pattern 4: No milliseconds in timestamp
    // 2025-06-27 12:30:34 [INF] Message
    private static readonly Regex NoMillisecondsRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s\[(?<lvl>VRB|DBG|INF|WRN|ERR|FTL)\]\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Pattern 5: Time-only timestamp with timezone
    // 22:43:04.863 +05:00 [Information] [Microsoft.Hosting.Lifetime] Message
    private static readonly Regex TimeOnlyRegex = new(
        @"^(?<ts>\d{2}:\d{2}:\d{2}\.\d+\s[+-]\d{2}:\d{2})\s\[(?<lvl>Verbose|Debug|Information|Warning|Error|Fatal|VRB|DBG|INF|WRN|ERR|FTL)\](?:\s\[[^\]]*\])?\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Pattern 6: Custom format with level first and || separator
    // [Warning] 28-06 02:35:34 || Message
    private static readonly Regex CustomSeparatorRegex = new(
        @"^\[(?<lvl>Verbose|Debug|Information|Warning|Error|Fatal|VRB|DBG|INF|WRN|ERR|FTL)\]\s(?<ts>\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s\|\|\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines)
    {
        var lines = sampleLines.Split('\n').Take(10);
        var matchCount = lines.Count(l =>
        {
            var trimmed = l.Trim();
            return Iso8601Regex.IsMatch(trimmed) ||
                   BracketedRegex.IsMatch(trimmed) ||
                   SpaceSeparatedRegex.IsMatch(trimmed) ||
                   NoMillisecondsRegex.IsMatch(trimmed) ||
                   TimeOnlyRegex.IsMatch(trimmed) ||
                   CustomSeparatorRegex.IsMatch(trimmed);
        });
        return matchCount >= 2;
    }

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
    {
        var lines = rawLog.Split('\n');
        var lineNum = 0;

        foreach (var group in GroupMultilineEntries(lines, IsNewEntry))
        {
            lineNum++;
            var trimmed = group[0].Trim();

            // Try each pattern in order (ISO8601 first as it's most specific)
            var match = Iso8601Regex.Match(trimmed);
            if (!match.Success) match = BracketedRegex.Match(trimmed);
            if (!match.Success) match = SpaceSeparatedRegex.Match(trimmed);
            if (!match.Success) match = NoMillisecondsRegex.Match(trimmed);
            if (!match.Success) match = TimeOnlyRegex.Match(trimmed);
            if (!match.Success) match = CustomSeparatorRegex.Match(trimmed);

            if (!match.Success) continue;

            var fullMsg = group.Length > 1
                ? match.Groups["msg"].Value + "\n" + string.Join("\n", group.Skip(1))
                : match.Groups["msg"].Value;

            var (body, stack) = SplitStackTrace(fullMsg);
            var exceptionType = ExtractExceptionType(body);

            var structured = new StructuredLogEntry
            {
                Timestamp = DateTime.TryParse(match.Groups["ts"].Value, out var ts) ? ts : null,
                Level = ParseLevel(match.Groups["lvl"].Value),
                Message = body,
                LineNumber = lineNum,
                RawLine = string.Join(Environment.NewLine, group),
                ParserType = FormatName,
                FieldSections = []
            };

            var sections = new List<FieldSection>();
            var sectionOrder = 0;

            // Section 1: Exception Details
            var exceptionSection = CreateExceptionSection(exceptionType, stack, sectionOrder++);
            if (exceptionSection != null)
                sections.Add(exceptionSection);

            structured.FieldSections = sections;
            yield return structured;
        }
    }

    private static bool IsNewEntry(string line)
    {
        var trimmed = line.Trim();
        return Iso8601Regex.IsMatch(trimmed) ||
               BracketedRegex.IsMatch(trimmed) ||
               SpaceSeparatedRegex.IsMatch(trimmed) ||
               NoMillisecondsRegex.IsMatch(trimmed) ||
               TimeOnlyRegex.IsMatch(trimmed) ||
               CustomSeparatorRegex.IsMatch(trimmed);
    }
}
