using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// Handles: 2024-03-14 02:14:33.123 +05:00 [ERR] Message
public class SerilogFormatParser : BaseLogParser
{
    public override string FormatName => "Serilog";

    private static readonly Regex EntryRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d+\s[+-]\d{2}:\d{2}|\[\d{2}:\d{2}:\d{2}\])\s\[(?<lvl>VRB|DBG|INF|WRN|ERR|FTL)\]\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines)
    {
        var lines = sampleLines.Split('\n').Take(10);
        return lines.Count(l => EntryRegex.IsMatch(l.Trim())) >= 2;
    }

    public override IEnumerable<LogEntry> Parse(string rawLog)
    {
        var lines = rawLog.Split('\n');
        var lineNum = 0;

        foreach (var group in GroupMultilineEntries(lines, l => EntryRegex.IsMatch(l.Trim())))
        {
            lineNum++;
            var m = EntryRegex.Match(group[0].Trim());
            if (!m.Success) continue;

            var fullMsg = group.Length > 1
                ? m.Groups["msg"].Value + "\n" + string.Join("\n", group.Skip(1))
                : m.Groups["msg"].Value;

            var (body, stack) = SplitStackTrace(fullMsg);

            yield return new LogEntry
            {
                Timestamp = DateTime.TryParse(m.Groups["ts"].Value.Trim('[', ']'), out var ts) ? ts : null,
                Level = ParseLevel(m.Groups["lvl"].Value),
                Message = body,
                StackTrace = stack,
                ExceptionType = ExtractExceptionType(body),
                LineNumber = lineNum,
                RawLine = string.Join(Environment.NewLine, group)
            };
        }
    }
}
