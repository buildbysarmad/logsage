using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// NLog: 2024-03-14 02:14:33.1234|ERROR|MyApp.Service|Message
public class NLogFormatParser : BaseLogParser
{
    public override string FormatName => "NLog";

    private static readonly Regex EntryRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d+)\|(?<lvl>TRACE|DEBUG|INFO|WARN|ERROR|FATAL)\|(?<src>[^|]+)\|(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines) =>
        sampleLines.Split('\n').Take(10).Count(l => EntryRegex.IsMatch(l.Trim())) >= 2;

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
                Timestamp = DateTime.TryParse(m.Groups["ts"].Value, out var ts) ? ts : null,
                Level = ParseLevel(m.Groups["lvl"].Value),
                Message = body,
                StackTrace = stack,
                Source = m.Groups["src"].Value.Trim(),
                ExceptionType = ExtractExceptionType(body),
                LineNumber = lineNum,
                RawLine = string.Join(Environment.NewLine, group)
            };
        }
    }
}

// Log4Net: 2024-03-14 02:14:33,123 ERROR [MyApp.Service] Message
public class Log4NetFormatParser : BaseLogParser
{
    public override string FormatName => "Log4Net";

    private static readonly Regex EntryRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2},\d+)\s(?<lvl>DEBUG|INFO|WARN|ERROR|FATAL)\s+\[(?<src>[^\]]+)\]\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines) =>
        sampleLines.Split('\n').Take(10).Count(l => EntryRegex.IsMatch(l.Trim())) >= 2;

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
                Timestamp = DateTime.TryParse(m.Groups["ts"].Value.Replace(',', '.'), out var ts) ? ts : null,
                Level = ParseLevel(m.Groups["lvl"].Value),
                Message = body,
                StackTrace = stack,
                Source = m.Groups["src"].Value.Trim(),
                ExceptionType = ExtractExceptionType(body),
                LineNumber = lineNum,
                RawLine = string.Join(Environment.NewLine, group)
            };
        }
    }
}
