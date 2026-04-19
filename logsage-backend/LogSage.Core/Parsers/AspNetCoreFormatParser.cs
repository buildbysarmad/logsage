using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

// Handles: warn: Microsoft.AspNetCore.Server.Kestrel[0]
//                Overriding address(es) 'http://localhost:5151'...
//          info: Microsoft.Hosting.Lifetime[14]
//                Now listening on: http://[::]:8081
public class AspNetCoreFormatParser : BaseStructuredLogParser, ILogParser
{
    public override string FormatName => "ASP.NET Core";

    private static readonly Regex EntryRegex = new(
        @"^(?<lvl>trce|dbug|info|warn|fail|crit):\s*(?<category>[^\[]+)\[(?<eventId>\d+)\](?:\s*(?<msg>.*))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines)
    {
        var lines = sampleLines.Split('\n').Take(10);
        var matchCount = lines.Count(l => EntryRegex.IsMatch(l.Trim()));
        return matchCount >= 2;
    }

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

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
    {
        var lines = rawLog.Split('\n');
        var lineNum = 0;

        foreach (var group in GroupMultilineEntries(lines, l => EntryRegex.IsMatch(l.Trim())))
        {
            lineNum++;
            var m = EntryRegex.Match(group[0].Trim());
            if (!m.Success) continue;

            var category = m.Groups["category"].Value.Trim();
            var eventId = m.Groups["eventId"].Value;
            var firstLineMsg = m.Groups["msg"].Value.Trim();

            // Collect continuation lines (indented lines after first line)
            var continuationLines = group.Skip(1)
                .Select(l => l.TrimStart())
                .Where(l => !string.IsNullOrWhiteSpace(l));

            var fullMsg = string.IsNullOrWhiteSpace(firstLineMsg)
                ? string.Join("\n", continuationLines)
                : firstLineMsg + (continuationLines.Any() ? "\n" + string.Join("\n", continuationLines) : "");

            var (body, stack) = SplitStackTrace(fullMsg);
            var exceptionType = ExtractExceptionType(body);

            var structured = new StructuredLogEntry
            {
                Timestamp = null, // ASP.NET Core default format doesn't include timestamps
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
            var contextSection = CreateSection("Logger Context", sectionOrder++);
            AddFieldIfPresent(contextSection, "category", "Category", category,
                FieldType.Text, FieldImportance.Secondary);
            AddFieldIfPresent(contextSection, "eventId", "Event ID", eventId,
                FieldType.Number, FieldImportance.Secondary);
            if (contextSection.Fields.Count > 0)
                sections.Add(contextSection);

            // Section 2: Exception Details
            var exceptionSection = CreateExceptionSection(exceptionType, stack, sectionOrder++);
            if (exceptionSection != null)
                sections.Add(exceptionSection);

            structured.FieldSections = sections;
            yield return structured;
        }
    }
}
