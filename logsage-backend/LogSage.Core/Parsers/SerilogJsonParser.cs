using System.Linq;
using System.Text.Json;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

/// <summary>
/// Parses Serilog JSON/CLEF (Compact Log Event Format) logs.
/// </summary>
/// <remarks>
/// Handles formats:
/// - Standard: {"Timestamp":"2020-09-13T17:53:07.1995663+03:00","Level":"Information","MessageTemplate":"...","Properties":{...}}
/// - CLEF: {"@t":"2026-03-26T10:15:38.2135938Z","@m":"Message","@l":"Information","@i":"d826f4b8",...}
/// </remarks>
public class SerilogJsonParser : BaseStructuredLogParser, ILogParser
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

    public override bool CanParse(string sampleLines)
    {
        var lines = sampleLines.Split('\n').Take(10);
        var matchCount = 0;

        foreach (var trimmed in lines.Select(line => line.Trim()))
        {
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;

                // Check for standard Serilog JSON format (Timestamp + Level)
                var isStandardFormat = root.TryGetProperty("Timestamp", out _) &&
                                      root.TryGetProperty("Level", out _);

                // Check for CLEF (Compact Log Event Format) with @t and @m
                var isClefFormat = root.TryGetProperty("@t", out _) &&
                                  root.TryGetProperty("@m", out _);

                if (isStandardFormat || isClefFormat)
                {
                    matchCount++;
                }
            }
            catch (JsonException)
            {
                // Not valid JSON, skip
            }
        }

        return matchCount >= 2;
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

            StructuredLogEntry? entry = null;
            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;

                // Extract timestamp (standard or CLEF format)
                DateTime? timestamp = null;
                if (root.TryGetProperty("Timestamp", out var tsElement) &&
                    DateTime.TryParse(tsElement.GetString(), out var ts))
                {
                    timestamp = ts;
                }
                else if (root.TryGetProperty("@t", out var clefTsElement) &&
                         DateTime.TryParse(clefTsElement.GetString(), out var ts))
                {
                    timestamp = ts;
                }

                // Extract level (standard or CLEF format)
                var level = LogLevel.Unknown;
                if (root.TryGetProperty("Level", out var lvlElement))
                {
                    level = ParseLevel(lvlElement.GetString() ?? "");
                }
                else if (root.TryGetProperty("@l", out var clefLvlElement))
                {
                    level = ParseLevel(clefLvlElement.GetString() ?? "");
                }

                // Extract message - try MessageTemplate, @m (CLEF), then Message
                var message = "";
                if (root.TryGetProperty("MessageTemplate", out var msgTemplateElement))
                {
                    message = msgTemplateElement.GetString() ?? "";
                }
                else if (root.TryGetProperty("@m", out var clefMsgElement))
                {
                    message = clefMsgElement.GetString() ?? "";
                }
                else if (root.TryGetProperty("Message", out var msgElement))
                {
                    message = msgElement.GetString() ?? "";
                }

                // Check for exception in Properties
                string? exceptionType = null;
                string? stackTrace = null;
                if (root.TryGetProperty("Properties", out var propsElement))
                {
                    if (propsElement.TryGetProperty("ExceptionDetail", out var exElement) ||
                        propsElement.TryGetProperty("Exception", out exElement))
                    {
                        var exText = exElement.GetString() ?? "";
                        exceptionType = ExtractExceptionType(exText);
                        stackTrace = exText.Contains("\n   at ") ? exText : null;
                    }
                }

                // Also check for exception in message
                if (exceptionType == null)
                {
                    exceptionType = ExtractExceptionType(message);
                }

                entry = new StructuredLogEntry
                {
                    Timestamp = timestamp,
                    Level = level,
                    Message = message,
                    LineNumber = lineNum,
                    RawLine = trimmed,
                    ParserType = FormatName,
                    FieldSections = []
                };

                var sections = new List<FieldSection>();
                var sectionOrder = 0;

                // Section 1: Exception Details
                var exceptionSection = CreateExceptionSection(exceptionType, stackTrace, sectionOrder++);
                if (exceptionSection != null)
                    sections.Add(exceptionSection);

                entry.FieldSections = sections;
            }
            catch (JsonException)
            {
                // Skip malformed JSON lines silently
            }

            if (entry != null)
                yield return entry;
        }
    }
}
