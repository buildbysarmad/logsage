namespace LogSage.Api.Data.Entities;

public enum ParseOutcome
{
    Success = 0,        // >80% success rate
    PartialSuccess = 1, // 20-80% success rate
    Failure = 2         // <20% success rate
}

public class ParseSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string SessionToken { get; init; } = string.Empty;  // 32-byte cryptorandom, base64url
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Input metadata
    public string? InputSample { get; init; }       // sanitized first 50 lines (max 500 chars)
    public int InputLineCount { get; init; }
    public int InputSizeBytes { get; init; }

    // JSONB metadata for efficient querying (user agent, IP country, etc.)
    public string? Metadata { get; set; }           // JSON serialized metadata

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

    // Outcome tracking
    public ParseOutcome Outcome { get; set; }

    /// <summary>
    /// Success rate as percentage (0-100)
    /// </summary>
    public double SuccessRate => InputLineCount > 0
        ? (double)TotalEntries / InputLineCount * 100
        : 0;

    // Feedback
    public int? FeedbackScore { get; set; }         // 1 = thumbs up, -1 = thumbs down
    public DateTime? FeedbackAt { get; set; }

    /// <summary>
    /// Calculate outcome based on success rate thresholds
    /// </summary>
    public static ParseOutcome CalculateOutcome(int totalEntries, int inputLineCount)
    {
        if (inputLineCount == 0) return ParseOutcome.Failure;

        var successRate = (double)totalEntries / inputLineCount * 100;

        return successRate switch
        {
            > 80 => ParseOutcome.Success,
            >= 20 => ParseOutcome.PartialSuccess,
            _ => ParseOutcome.Failure
        };
    }
}
