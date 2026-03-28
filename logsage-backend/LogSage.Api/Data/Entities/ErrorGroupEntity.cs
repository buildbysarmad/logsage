namespace LogSage.Api.Data.Entities;

public class ErrorGroupEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public string GroupKey { get; set; } = string.Empty;
    public string RepresentativeMessage { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? ExceptionType { get; set; }
    public DateTime? FirstSeen { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? AiSeverity { get; set; }
    public string? AiRootCause { get; set; }
    public string? AiSuggestedFix { get; set; }
    public Session Session { get; set; } = null!;
}
