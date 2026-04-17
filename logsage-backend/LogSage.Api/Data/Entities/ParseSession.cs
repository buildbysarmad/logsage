namespace LogSage.Api.Data.Entities;

public class ParseSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string SessionToken { get; init; } = string.Empty;  // 32-byte cryptorandom, base64url
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Input metadata
    public string? InputSample { get; init; }       // sanitized first 50 lines
    public int InputLineCount { get; init; }
    public int InputSizeBytes { get; init; }

    // Parse result
    public string DetectedFormat { get; set; } = string.Empty;
    public bool ParseSuccess { get; set; }
    public int TotalEntries { get; set; }
    public int InfoCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public int DebugCount { get; set; }
    public int ParseErrorCount { get; set; }
    public string? ParseErrorSamples { get; set; }  // JSON array, max 3 raw lines that failed
    public int DurationMs { get; set; }

    // Feedback
    public int? FeedbackScore { get; set; }         // 1 = thumbs up, -1 = thumbs down
    public DateTime? FeedbackAt { get; set; }
}
