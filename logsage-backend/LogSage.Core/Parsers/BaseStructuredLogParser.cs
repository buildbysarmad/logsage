using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

/// <summary>
/// Base class for parsers implementing the new structured log entry format.
/// Provides common utilities for building field sections.
/// </summary>
public abstract class BaseStructuredLogParser : IStructuredLogParser
{
    public abstract string FormatName { get; }
    public abstract bool CanParse(string sampleLines);
    public abstract IEnumerable<StructuredLogEntry> ParseStructured(string rawLog);

    // Legacy helper methods from BaseLogParser

    protected static LogLevel ParseLevel(string level) => level.ToUpperInvariant().Trim() switch
    {
        "TRACE" or "TRC" or "VERBOSE" or "VRB" => LogLevel.Trace,
        "DEBUG" or "DBG" => LogLevel.Debug,
        "INFO" or "INFORMATION" or "INF" => LogLevel.Info,
        "WARN" or "WARNING" or "WRN" => LogLevel.Warning,
        "ERROR" or "ERR" => LogLevel.Error,
        "FATAL" or "CRITICAL" or "FTL" or "CRIT" => LogLevel.Fatal,
        _ => LogLevel.Unknown
    };

    protected static string? ExtractExceptionType(string message)
    {
        var match = Regex.Match(message, @"([A-Z][a-zA-Z]+Exception)");
        return match.Success ? match.Value : null;
    }

    protected static (string body, string? stackTrace) SplitStackTrace(string message)
    {
        var idx = message.IndexOf("\n   at ", StringComparison.Ordinal);
        if (idx < 0) idx = message.IndexOf("\r\n   at ", StringComparison.Ordinal);
        if (idx < 0) return (message.Trim(), null);
        return (message[..idx].Trim(), message[idx..].Trim());
    }

    protected static IEnumerable<string[]> GroupMultilineEntries(
        string[] lines, Func<string, bool> isNewEntry)
    {
        var buffer = new List<string>();
        foreach (var line in lines)
        {
            if (isNewEntry(line) && buffer.Count > 0)
            {
                yield return buffer.ToArray();
                buffer.Clear();
            }
            buffer.Add(line);
        }
        if (buffer.Count > 0) yield return buffer.ToArray();
    }

    // New helper methods for building structured sections

    /// <summary>
    /// Creates a field section with the given name and order
    /// </summary>
    protected static FieldSection CreateSection(string name, int order)
    {
        return new FieldSection
        {
            SectionName = name,
            DisplayOrder = order,
            Fields = []
        };
    }

    /// <summary>
    /// Adds a field to a section if the value is not null/empty
    /// </summary>
    protected static void AddFieldIfPresent(
        FieldSection section,
        string key,
        string displayName,
        object? value,
        FieldType type = FieldType.Text,
        FieldImportance importance = FieldImportance.Secondary,
        Dictionary<string, object>? hints = null)
    {
        if (value == null) return;
        if (value is string str && string.IsNullOrWhiteSpace(str)) return;

        section.Fields.Add(new DisplayField
        {
            Key = key,
            DisplayName = displayName,
            Value = value,
            Type = type,
            Importance = importance,
            Hints = hints
        });
    }

    /// <summary>
    /// Creates an exception details section if exception type or stack trace present
    /// </summary>
    protected static FieldSection? CreateExceptionSection(string? exceptionType, string? stackTrace, int order)
    {
        if (string.IsNullOrEmpty(exceptionType) && string.IsNullOrEmpty(stackTrace))
            return null;

        var section = CreateSection("Exception Details", order);

        AddFieldIfPresent(section, "exceptionType", "Exception Type", exceptionType,
            FieldType.ExceptionType, FieldImportance.Primary);

        AddFieldIfPresent(section, "stackTrace", "Stack Trace", stackTrace,
            FieldType.StackTrace, FieldImportance.Secondary);

        return section.Fields.Count > 0 ? section : null;
    }

    /// <summary>
    /// Creates a source/logger context section if source is present
    /// </summary>
    protected static FieldSection? CreateSourceSection(string? source, int order, string sectionName = "Source Context")
    {
        if (string.IsNullOrWhiteSpace(source))
            return null;

        var section = CreateSection(sectionName, order);
        AddFieldIfPresent(section, "source", "Source", source,
            FieldType.Text, FieldImportance.Secondary);

        return section.Fields.Count > 0 ? section : null;
    }
}
