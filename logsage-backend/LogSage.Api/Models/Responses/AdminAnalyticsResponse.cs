namespace LogSage.Api.Models.Responses;

public record AdminAnalyticsResponse(
    AnalyticsOverview Overview,
    List<DailyStats> DailyTrend,
    List<FormatBreakdown> FormatBreakdown,
    List<OutcomeBreakdown> OutcomeBreakdown
);

public record AnalyticsOverview(
    int TotalSessions,
    double AverageSuccessRate,
    int TotalErrorsDetected,
    int UniqueFormatsDetected,
    long TotalBytesProcessed,
    int AverageDurationMs
);

public record DailyStats(
    DateOnly Date,
    int SessionCount,
    double AverageSuccessRate,
    int TotalErrors
);

public record FormatBreakdown(
    string Format,
    int Count,
    double Percentage
);

public record OutcomeBreakdown(
    string Outcome,
    int Count,
    double Percentage,
    double AverageSuccessRate
);
