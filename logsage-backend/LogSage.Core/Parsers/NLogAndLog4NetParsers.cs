using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// NLog: 2024-03-14 02:14:33.1234|ERROR|MyApp.Service|Message
public class NLogFormatParser : BaseStructuredLogParser, ILogParser
{
    public override string FormatName => "NLog";

    private static readonly Regex EntryRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d+)\|(?<lvl>TRACE|DEBUG|INFO|WARN|ERROR|FATAL)\|(?<src>[^|]+)\|(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines) =>
        sampleLines.Split('\n').Take(10).Count(l => EntryRegex.IsMatch(l.Trim())) >= 2;

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
                Source = structured.Source,
                ExceptionType = structured.ExceptionType
            };
        }
    }

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
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
            var source = m.Groups["src"].Value.Trim();
            var exceptionType = ExtractExceptionType(body);

            var structured = new StructuredLogEntry
            {
                Timestamp = DateTime.TryParse(m.Groups["ts"].Value, out var ts) ? ts : null,
                Level = ParseLevel(m.Groups["lvl"].Value),
                Message = body,
                LineNumber = lineNum,
                RawLine = string.Join(Environment.NewLine, group),
                ParserType = FormatName,
                FieldSections = []
            };

            var sections = new List<FieldSection>();
            var sectionOrder = 0;

            // Section 1: Logger Context
            var loggerSection = CreateSourceSection(source, sectionOrder++, "Logger Context");
            if (loggerSection != null)
                sections.Add(loggerSection);

            // Section 2: Exception Details
            var exceptionSection = CreateExceptionSection(exceptionType, stack, sectionOrder++);
            if (exceptionSection != null)
                sections.Add(exceptionSection);

            structured.FieldSections = sections;
            yield return structured;
        }
    }
}

// Log4Net: 2024-03-14 02:14:33,123 ERROR [MyApp.Service] Message
// Also handles: INFORMATION, WARNING (long forms)
public class Log4NetFormatParser : BaseStructuredLogParser, ILogParser
{
    public override string FormatName => "Log4Net";

    private static readonly Regex EntryRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2},\d+)\s(?<lvl>TRACE|DEBUG|INFO|INFORMATION|WARN|WARNING|ERROR|FATAL)\s+\[(?<src>[^\]]+)\]\s(?<msg>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines) =>
        sampleLines.Split('\n').Take(10).Count(l => EntryRegex.IsMatch(l.Trim())) >= 2;

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
                Source = structured.Source,
                ExceptionType = structured.ExceptionType
            };
        }
    }

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
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
            var source = m.Groups["src"].Value.Trim();
            var exceptionType = ExtractExceptionType(body);

            var structured = new StructuredLogEntry
            {
                Timestamp = DateTime.TryParse(m.Groups["ts"].Value.Replace(',', '.'), out var ts) ? ts : null,
                Level = ParseLevel(m.Groups["lvl"].Value),
                Message = body,
                LineNumber = lineNum,
                RawLine = string.Join(Environment.NewLine, group),
                ParserType = FormatName,
                FieldSections = []
            };

            var sections = new List<FieldSection>();
            var sectionOrder = 0;

            // Section 1: Logger Context
            var loggerSection = CreateSourceSection(source, sectionOrder++, "Logger Context");
            if (loggerSection != null)
                sections.Add(loggerSection);

            // Section 2: Exception Details
            var exceptionSection = CreateExceptionSection(exceptionType, stack, sectionOrder++);
            if (exceptionSection != null)
                sections.Add(exceptionSection);

            structured.FieldSections = sections;
            yield return structured;
        }
    }
}
