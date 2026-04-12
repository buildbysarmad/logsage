namespace LogSage.Core.Models;

/// <summary>
/// Adapter to convert legacy LogEntry to StructuredLogEntry during migration.
/// This allows old parsers to work with new code until they're refactored.
/// </summary>
public static class LogEntryAdapter
{
    /// <summary>
    /// Converts a legacy LogEntry to a StructuredLogEntry with basic sections
    /// </summary>
    public static StructuredLogEntry ToStructured(this LogEntry entry, string parserType)
    {
        var structured = new StructuredLogEntry
        {
            Timestamp = entry.Timestamp,
            Level = entry.Level,
            Message = entry.Message,
            LineNumber = entry.LineNumber,
            RawLine = entry.RawLine,
            ParserType = parserType,
            ParseError = entry.ParseError,
            ParseErrorMessage = entry.ParseErrorMessage,
            FieldSections = []
        };

        // Build sections based on available fields

        var sections = new List<FieldSection>();
        var sectionOrder = 0;

        // Section 1: Source Context (if source present)
        if (!string.IsNullOrWhiteSpace(entry.Source))
        {
            var sourceSection = new FieldSection
            {
                SectionName = "Source Context",
                DisplayOrder = sectionOrder++,
                Fields = [
                    new DisplayField
                    {
                        Key = "source",
                        DisplayName = "Source",
                        Value = entry.Source,
                        Type = FieldType.Text,
                        Importance = FieldImportance.Secondary
                    }
                ]
            };
            sections.Add(sourceSection);
        }

        // Section 2: Serilog Structured Fields (if present)
        if (entry.StructuredFields.Count > 0)
        {
            var structuredSection = new FieldSection
            {
                SectionName = "Request Context",
                DisplayOrder = sectionOrder++,
                Fields = []
            };

            // Add common Serilog fields with proper types
            if (entry.RequestId != null)
            {
                structuredSection.Fields.Add(new DisplayField
                {
                    Key = "requestId",
                    DisplayName = "Request ID",
                    Value = entry.RequestId,
                    Type = FieldType.Text,
                    Importance = FieldImportance.Secondary
                });
            }

            if (entry.RequestPath != null)
            {
                structuredSection.Fields.Add(new DisplayField
                {
                    Key = "requestPath",
                    DisplayName = "Request Path",
                    Value = entry.RequestPath,
                    Type = FieldType.Url,
                    Importance = FieldImportance.Secondary
                });
            }

            if (entry.ConnectionId != null)
            {
                structuredSection.Fields.Add(new DisplayField
                {
                    Key = "connectionId",
                    DisplayName = "Connection ID",
                    Value = entry.ConnectionId,
                    Type = FieldType.Text,
                    Importance = FieldImportance.Debug
                });
            }

            if (entry.StatusCode.HasValue)
            {
                var hints = entry.StatusCode >= 400
                    ? new Dictionary<string, object> { ["color"] = "red", ["colorThreshold"] = 400 }
                    : null;

                structuredSection.Fields.Add(new DisplayField
                {
                    Key = "statusCode",
                    DisplayName = "HTTP Status",
                    Value = entry.StatusCode.Value,
                    Type = FieldType.Number,
                    Importance = FieldImportance.Primary,
                    Hints = hints
                });
            }

            if (entry.SourceContext != null)
            {
                structuredSection.Fields.Add(new DisplayField
                {
                    Key = "sourceContext",
                    DisplayName = "Source Context",
                    Value = entry.SourceContext,
                    Type = FieldType.Text,
                    Importance = FieldImportance.Secondary
                });
            }

            // Add remaining structured fields
            foreach (var (key, value) in entry.StructuredFields)
            {
                // Skip fields we've already added
                if (key is "RequestId" or "RequestPath" or "ConnectionId" or "StatusCode" or "SourceContext")
                    continue;

                structuredSection.Fields.Add(new DisplayField
                {
                    Key = key,
                    DisplayName = FormatFieldName(key),
                    Value = value,
                    Type = FieldType.Text,
                    Importance = FieldImportance.Debug
                });
            }

            if (structuredSection.Fields.Count > 0)
                sections.Add(structuredSection);
        }

        // Section 3: Exception Details (if exception or stack trace present)
        if (!string.IsNullOrWhiteSpace(entry.ExceptionType) || !string.IsNullOrWhiteSpace(entry.StackTrace))
        {
            var exceptionSection = new FieldSection
            {
                SectionName = "Exception Details",
                DisplayOrder = sectionOrder++,
                Fields = []
            };

            if (!string.IsNullOrWhiteSpace(entry.ExceptionType))
            {
                exceptionSection.Fields.Add(new DisplayField
                {
                    Key = "exceptionType",
                    DisplayName = "Exception Type",
                    Value = entry.ExceptionType,
                    Type = FieldType.ExceptionType,
                    Importance = FieldImportance.Primary
                });
            }

            if (!string.IsNullOrWhiteSpace(entry.StackTrace))
            {
                exceptionSection.Fields.Add(new DisplayField
                {
                    Key = "stackTrace",
                    DisplayName = "Stack Trace",
                    Value = entry.StackTrace,
                    Type = FieldType.StackTrace,
                    Importance = FieldImportance.Secondary
                });
            }

            sections.Add(exceptionSection);
        }

        structured.FieldSections = sections;
        return structured;
    }

    private static string FormatFieldName(string key)
    {
        // Convert camelCase or PascalCase to "Title Case"
        if (string.IsNullOrWhiteSpace(key)) return key;

        var result = System.Text.RegularExpressions.Regex.Replace(
            key,
            @"([A-Z])",
            " $1"
        ).Trim();

        return char.ToUpper(result[0]) + result[1..];
    }
}
