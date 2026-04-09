using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// Handles: 2024-03-14 02:14:33 [ERROR] Message
//          2024-03-14T02:14:33 ERROR Message
public class StandardFormatParser : BaseLogParser
{
    public override string FormatName => "Standard";

    private static readonly Regex EntryRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?)?[\s\|]*\[?(?<lvl>TRACE|DEBUG|INFO|INFORMATION|WARN|WARNING|ERROR|FATAL|CRITICAL)\]?[\s\|]*(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StartsWithTs = new(@"^\d{4}-\d{2}-\d{2}", RegexOptions.Compiled);

    public override bool CanParse(string sampleLines)
    {
        var lines = sampleLines.Split('\n').Take(10);
        return lines.Count(l => EntryRegex.IsMatch(l.Trim())) >= 2;
    }

    public override IEnumerable<LogEntry> Parse(string rawLog)
    {
        var lines = rawLog.Split('\n');
        var lineNum = 0;

        foreach (var group in GroupMultilineEntries(lines,
            l => StartsWithTs.IsMatch(l.Trim()) ||
                 Regex.IsMatch(l, @"^\[?(ERROR|WARN|INFO|DEBUG|FATAL)", RegexOptions.IgnoreCase)))
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
                Timestamp = DateTime.TryParse(m.Groups["ts"].Value, out var ts) ? ts : null,
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
