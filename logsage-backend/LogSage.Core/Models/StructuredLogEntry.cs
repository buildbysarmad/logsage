namespace LogSage.Core.Models;

/// <summary>
/// Unified log entry structure with parser-specific field sections.
/// Replaces the flat LogEntry model with a structured, display-ready format.
/// </summary>
public class StructuredLogEntry
{
    // Core fields (all parsers must provide)

    /// <summary>Entry timestamp (nullable for formats without timestamps)</summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>Log level (Error, Warning, Info, etc.)</summary>
    public LogLevel Level { get; set; }

    /// <summary>Main log message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Line number in original log file</summary>
    public int LineNumber { get; set; }

    /// <summary>Original raw line(s) from log file</summary>
    public string RawLine { get; set; } = string.Empty;

    // Parser metadata

    /// <summary>Parser that processed this entry (e.g., "Serilog", "NLog")</summary>
    public string ParserType { get; set; } = string.Empty;

    /// <summary>Whether this entry had parse errors</summary>
    public bool ParseError { get; set; }

    /// <summary>Parse error message if ParseError is true</summary>
    public string? ParseErrorMessage { get; set; }

    // Display-ready field sections (parser-specific)

    /// <summary>
    /// Parser-specific field sections with display metadata.
    /// Frontend renders these generically without knowing parser type.
    /// </summary>
    public List<FieldSection> FieldSections { get; set; } = [];

    // Computed properties for backward compatibility during migration

    /// <summary>Exception type extracted from message or stack trace (legacy)</summary>
    public string? ExceptionType =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "exceptionType")?.Value?.ToString();

    /// <summary>Stack trace if present (legacy)</summary>
    public string? StackTrace =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "stackTrace")?.Value?.ToString();

    /// <summary>Source/logger name if present (legacy)</summary>
    public string? Source =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "source" || f.Key == "sourceContext")?.Value?.ToString();

    /// <summary>Serilog: Request ID field (legacy)</summary>
    public string? RequestId =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "requestId")?.Value?.ToString();

    /// <summary>Serilog: Request path field (legacy)</summary>
    public string? RequestPath =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "requestPath")?.Value?.ToString();

    /// <summary>Serilog: Connection ID field (legacy)</summary>
    public string? ConnectionId =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "connectionId")?.Value?.ToString();

    /// <summary>Serilog: HTTP status code field (legacy)</summary>
    public int? StatusCode
    {
        get
        {
            var field = FieldSections
                .SelectMany(s => s.Fields)
                .FirstOrDefault(f => f.Key == "statusCode");
            if (field?.Value == null) return null;
            return field.Value is int i ? i : int.TryParse(field.Value.ToString(), out var parsed) ? parsed : null;
        }
    }

    /// <summary>Serilog: Source context field (legacy)</summary>
    public string? SourceContext =>
        FieldSections
            .SelectMany(s => s.Fields)
            .FirstOrDefault(f => f.Key == "sourceContext")?.Value?.ToString();

    /// <summary>All structured fields as dictionary (legacy - reconstructed from FieldSections)</summary>
    public Dictionary<string, object> StructuredFields
    {
        get
        {
            var fields = new Dictionary<string, object>();
            foreach (var section in FieldSections)
            {
                foreach (var field in section.Fields)
                {
                    if (field.Value != null)
                        fields[field.Key] = field.Value;
                }
            }
            return fields;
        }
    }
}
