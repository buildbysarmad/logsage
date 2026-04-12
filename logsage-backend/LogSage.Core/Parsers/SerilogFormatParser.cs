using System.Text.Json;
using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

/// <summary>
/// Handles real-world Serilog format with multi-line entries:
/// [HH:MM:SS LVL] [LVL] Message
/// {JSON_payload_or_{}}
/// [optional stack trace lines]
/// [blank separator]
/// </summary>
public class SerilogFormatParser : BaseStructuredLogParser, ILogParser
{
    public override string FormatName => "Serilog";

    // Matches: [01:43:13 INF] [INF] Message  OR  2024-03-14 02:14:33.123 +05:00 [ERR] Message
    private static readonly Regex EntryHeaderRegex = new(
        @"^\[(?<time>\d{2}:\d{2}:\d{2})\s+(?<lvl1>VRB|DBG|INF|WRN|ERR|FTL)\]\s*\[(?<lvl2>VRB|DBG|INF|WRN|ERR|FTL)\]\s*(?<msg>.*)|" +
        @"^(?<fullts>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d+\s+[+-]\d{2}:\d{2})\s+\[(?<lvl>VRB|DBG|INF|WRN|ERR|FTL)\]\s+(?<msg2>.*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override bool CanParse(string sampleLines)
    {
        // Normalize line endings first
        var normalized = NormalizeLineEndings(sampleLines);
        var lines = normalized.Split('\n', StringSplitOptions.None).Take(20);

        var matchCount = 0;
        foreach (var line in lines)
        {
            if (EntryHeaderRegex.IsMatch(line.Trim()))
                matchCount++;
        }

        return matchCount >= 2;
    }

    // Legacy ILogParser implementation - kept for backward compatibility
    public IEnumerable<LogEntry> Parse(string rawLog)
    {
        foreach (var structured in ParseStructured(rawLog))
        {
            // Convert StructuredLogEntry back to legacy LogEntry
            yield return new LogEntry
            {
                Timestamp = structured.Timestamp,
                Level = structured.Level,
                Message = structured.Message,
                LineNumber = structured.LineNumber,
                RawLine = structured.RawLine,
                ParseError = structured.ParseError,
                ParseErrorMessage = structured.ParseErrorMessage,
                StackTrace = structured.StackTrace,
                Source = structured.Source,
                ExceptionType = structured.ExceptionType,
                StructuredFields = ExtractStructuredFieldsFromSections(structured.FieldSections)
            };
        }
    }

    public override IEnumerable<StructuredLogEntry> ParseStructured(string rawLog)
    {
        // Normalize line endings: \r\n -> \n
        var normalized = NormalizeLineEndings(rawLog);
        var lines = normalized.Split('\n', StringSplitOptions.None);

        var lineNum = 0;
        var i = 0;

        while (i < lines.Length)
        {
            var line = lines[i];
            var match = EntryHeaderRegex.Match(line.Trim());

            if (!match.Success)
            {
                i++;
                continue;
            }

            lineNum++;
            var entryLines = new List<string> { line };
            i++;

            // Collect lines until next entry header or end of file
            // Real-world Serilog has: header, JSON, blank, stack trace, blank separator
            // So we need to collect everything until we hit the next header
            var consecutiveBlankLines = 0;
            while (i < lines.Length)
            {
                var currentLine = lines[i];

                // Check if this is the start of a new entry
                if (EntryHeaderRegex.IsMatch(currentLine.Trim()))
                    break;

                // Track consecutive blank lines - if we hit 2+, that's likely the separator
                if (string.IsNullOrWhiteSpace(currentLine))
                {
                    consecutiveBlankLines++;
                    if (consecutiveBlankLines >= 2)
                    {
                        i++;
                        break; // End of entry
                    }
                }
                else
                {
                    consecutiveBlankLines = 0;
                }

                entryLines.Add(currentLine);
                i++;
            }

            // Parse the entry
            var entry = ParseEntryStructured(entryLines, lineNum);
            if (entry != null)
                yield return entry;
        }
    }

    private StructuredLogEntry? ParseEntryStructured(List<string> entryLines, int lineNum)
    {
        if (entryLines.Count == 0)
            return null;

        var headerLine = entryLines[0];
        var match = EntryHeaderRegex.Match(headerLine.Trim());

        if (!match.Success)
            return null;

        // Extract timestamp and level
        DateTime? timestamp = null;
        LogLevel level;
        string message;

        if (match.Groups["fullts"].Success && !string.IsNullOrEmpty(match.Groups["fullts"].Value))
        {
            // Full timestamp format: 2024-03-14 02:14:33.123 +05:00 [ERR] Message
            if (DateTime.TryParse(match.Groups["fullts"].Value, out var ts))
                timestamp = ts;
            level = ParseLevel(match.Groups["lvl"].Value);
            message = match.Groups["msg2"].Value.Trim();
        }
        else
        {
            // Time-only format: [01:43:13 INF] [INF] Message
            var timeStr = match.Groups["time"].Value;
            if (TimeSpan.TryParse(timeStr, out var time))
            {
                // Use today's date + parsed time (user should provide date via filename if needed)
                timestamp = DateTime.Today.Add(time);
            }
            level = ParseLevel(match.Groups["lvl1"].Value); // Use first level, ignore duplicate
            message = match.Groups["msg"].Value.Trim();
        }

        // Parse JSON payload (line 2 if present)
        var structuredFields = new Dictionary<string, object>();
        var payloadLine = entryLines.Count > 1 ? entryLines[1].Trim() : null;
        var parseError = false;
        var parseErrorMessage = (string?)null;

        if (!string.IsNullOrEmpty(payloadLine) && payloadLine.StartsWith("{"))
        {
            try
            {
                if (payloadLine != "{}")
                {
                    using var doc = JsonDocument.Parse(payloadLine);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        // Extract simple values, skip nested objects for now
                        structuredFields[prop.Name] = ExtractJsonValue(prop.Value);
                    }
                }
            }
            catch (JsonException ex)
            {
                parseError = true;
                parseErrorMessage = $"JSON parse error: {ex.Message}";
            }
        }

        // Extract stack trace (lines after JSON payload, skipping blank lines)
        // Format: header, JSON, blank, stack trace lines, blank separator
        var stackTraceLines = new List<string>();
        var startCollecting = false;
        for (var i = 2; i < entryLines.Count; i++)
        {
            var line = entryLines[i];

            // Start collecting after we skip the first blank line after JSON
            if (string.IsNullOrWhiteSpace(line) && !startCollecting)
            {
                startCollecting = true;
                continue;
            }

            // If we've started collecting, add non-blank lines
            if (startCollecting && !string.IsNullOrWhiteSpace(line))
            {
                stackTraceLines.Add(line);
            }
        }

        var stackTrace = stackTraceLines.Count > 0
            ? string.Join(Environment.NewLine, stackTraceLines).Trim()
            : null;

        // Extract exception type from stack trace or message
        var exceptionType = ExtractExceptionType(stackTrace ?? message);

        // Update source from SourceContext if available
        var source = structuredFields.TryGetValue("SourceContext", out var sc)
            ? sc?.ToString()
            : null;

        // Build structured entry with field sections
        var structured = new StructuredLogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Message = message,
            LineNumber = lineNum,
            RawLine = string.Join(Environment.NewLine, entryLines),
            ParserType = FormatName,
            ParseError = parseError,
            ParseErrorMessage = parseErrorMessage,
            FieldSections = []
        };

        var sections = new List<FieldSection>();
        var sectionOrder = 0;

        // Section 1: Request Context (HTTP fields from Serilog JSON)
        var requestSection = CreateSection("Request Context", sectionOrder++);

        if (structuredFields.TryGetValue("RequestId", out var reqId) && reqId != null)
            AddFieldIfPresent(requestSection, "requestId", "Request ID", reqId.ToString(),
                FieldType.Text, FieldImportance.Secondary);

        if (structuredFields.TryGetValue("RequestPath", out var reqPath) && reqPath != null)
            AddFieldIfPresent(requestSection, "requestPath", "Request Path", reqPath.ToString(),
                FieldType.Url, FieldImportance.Secondary);

        if (structuredFields.TryGetValue("ConnectionId", out var connId) && connId != null)
            AddFieldIfPresent(requestSection, "connectionId", "Connection ID", connId.ToString(),
                FieldType.Text, FieldImportance.Debug);

        if (structuredFields.TryGetValue("StatusCode", out var statusCode) && statusCode != null)
        {
            var statusInt = statusCode is int si ? si : int.TryParse(statusCode.ToString(), out var parsed) ? parsed : (int?)null;
            if (statusInt.HasValue)
            {
                var hints = statusInt >= 400
                    ? new Dictionary<string, object> { ["color"] = "red", ["colorThreshold"] = 400 }
                    : null;
                AddFieldIfPresent(requestSection, "statusCode", "HTTP Status", statusInt.Value,
                    FieldType.Number, FieldImportance.Primary, hints);
            }
        }

        if (requestSection.Fields.Count > 0)
            sections.Add(requestSection);

        // Section 2: Source Context
        var sourceSection = CreateSection("Source Context", sectionOrder++);
        AddFieldIfPresent(sourceSection, "sourceContext", "Source Context", source,
            FieldType.Text, FieldImportance.Secondary);

        if (sourceSection.Fields.Count > 0)
            sections.Add(sourceSection);

        // Section 3: Exception Details
        var exceptionSection = CreateExceptionSection(exceptionType, stackTrace, sectionOrder++);
        if (exceptionSection != null)
            sections.Add(exceptionSection);

        // Section 4: Structured Data (remaining fields from JSON)
        var structuredDataSection = CreateSection("Structured Data", sectionOrder++);
        foreach (var (key, value) in structuredFields)
        {
            // Skip fields we've already added to other sections
            if (key is "RequestId" or "RequestPath" or "ConnectionId" or "StatusCode" or "SourceContext")
                continue;

            AddFieldIfPresent(structuredDataSection, key, FormatFieldName(key), value,
                FieldType.Text, FieldImportance.Debug);
        }

        if (structuredDataSection.Fields.Count > 0)
            sections.Add(structuredDataSection);

        structured.FieldSections = sections;
        return structured;
    }

    private static Dictionary<string, object> ExtractStructuredFieldsFromSections(List<FieldSection> sections)
    {
        var fields = new Dictionary<string, object>();

        // Extract all fields from all sections, using proper casing for Serilog fields
        foreach (var section in sections)
        {
            foreach (var field in section.Fields)
            {
                if (field.Value == null) continue;

                // Map back to original Serilog field names (PascalCase)
                var key = field.Key switch
                {
                    "requestId" => "RequestId",
                    "requestPath" => "RequestPath",
                    "connectionId" => "ConnectionId",
                    "statusCode" => "StatusCode",
                    "sourceContext" => "SourceContext",
                    _ => field.Key
                };

                fields[key] = field.Value;
            }
        }

        return fields;
    }

    private static string FormatFieldName(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return key;
        var result = System.Text.RegularExpressions.Regex.Replace(key, @"([A-Z])", " $1").Trim();
        return char.ToUpper(result[0]) + result[1..];
    }

    private static object ExtractJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => "null",
            // For objects/arrays, store as string to avoid deep nesting
            _ => element.GetRawText()
        };
    }

    private static string NormalizeLineEndings(string input)
    {
        return input.Replace("\r\n", "\n");
    }
}
