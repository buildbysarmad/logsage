using System.Collections.Generic;

namespace LogSage.Core.Models;

public enum LogLevel
{
    Trace, Debug, Info, Warning, Error, Fatal, Unknown
}

public class LogEntry
{
    public DateTime? Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public string? ExceptionType { get; set; }
    public int LineNumber { get; set; }
    public string RawLine { get; set; } = string.Empty;

    // Structured fields from Serilog JSON payload
    public Dictionary<string, object> StructuredFields { get; set; } = new();

    // Common structured field accessors
    public string? RequestId => StructuredFields.TryGetValue("RequestId", out var v) ? v?.ToString() : null;
    public string? RequestPath => StructuredFields.TryGetValue("RequestPath", out var v) ? v?.ToString() : null;
    public string? ConnectionId => StructuredFields.TryGetValue("ConnectionId", out var v) ? v?.ToString() : null;
    public int? StatusCode => StructuredFields.TryGetValue("StatusCode", out var v) && int.TryParse(v?.ToString(), out var code) ? code : null;
    public string? SourceContext => StructuredFields.TryGetValue("SourceContext", out var v) ? v?.ToString() : null;

    // Parse metadata
    public bool ParseError { get; set; }
    public string? ParseErrorMessage { get; set; }
}
