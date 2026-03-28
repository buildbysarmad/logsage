namespace LogLens.Core.Models;

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
}
