using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// Handles: 2024-03-14 02:14:33 [ERROR] Message
//          2024-03-14T02:14:33 ERROR Message
public class StandardFormatParser : BaseStructuredLogParser, ILogParser
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

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
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

            // Section 1: Exception Details (only section for standard format)
            var exceptionSection = CreateExceptionSection(exceptionType, stack, sectionOrder++);
            if (exceptionSection != null)
                sections.Add(exceptionSection);

            structured.FieldSections = sections;
            yield return structured;
        }
    }
}
