namespace LogSage.Core.Models;

public class ErrorGroup
{
    public string GroupKey { get; set; } = string.Empty;
    public string RepresentativeMessage { get; set; } = string.Empty;
    public LogLevel Level { get; set; }
    public int Count { get; set; }
    public DateTime? FirstSeen { get; set; }
    public DateTime? LastSeen { get; set; }
    public List<StructuredLogEntry> Entries { get; set; } = [];
    public string? ExceptionType { get; set; }
    public string? Source { get; set; }
}
