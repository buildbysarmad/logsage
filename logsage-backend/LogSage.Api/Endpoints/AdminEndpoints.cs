using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        app.MapGet("/api/admin/analytics", GetAnalytics)
           .WithTags("Admin")
           .WithSummary("Get platform analytics — admin authentication required")
           .WithDescription("Returns aggregated analytics: total sessions, success rates, format breakdown, and daily trends. Requires authenticated admin user.")
           .RequireAuthorization(policy => policy.RequireRole("admin"));
    }

    private static async Task<IResult> GetAnalytics(
        AppDbContext db, HttpContext ctx, int days = 30, CancellationToken ct = default)
    {
        if (days < 1 || days > 365)
            return Results.BadRequest(new { error = "Days must be between 1 and 365" });

        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var sessions = await db.ParseSessions
            .AsNoTracking()
            .Where(s => s.CreatedAt >= cutoffDate)
            .ToListAsync(ct);

        if (sessions.Count == 0)
        {
            return Results.Ok(new AdminAnalyticsResponse(
                new AnalyticsOverview(0, 0, 0, 0, 0, 0),
                [],
                [],
                []
            ));
        }

        // Calculate overview metrics
        var overview = new AnalyticsOverview(
            TotalSessions: sessions.Count,
            AverageSuccessRate: sessions.Average(s => s.SuccessRate),
            TotalErrorsDetected: sessions.Sum(s => s.ErrorCount),
            UniqueFormatsDetected: sessions.Select(s => s.DetectedFormat).Distinct().Count(),
            TotalBytesProcessed: sessions.Sum(s => (long)s.InputSizeBytes),
            AverageDurationMs: (int)sessions.Average(s => s.DurationMs)
        );

        // Daily trend (last N days)
        var dailyTrend = sessions
            .GroupBy(s => DateOnly.FromDateTime(s.CreatedAt))
            .Select(g => new DailyStats(
                Date: g.Key,
                SessionCount: g.Count(),
                AverageSuccessRate: g.Average(s => s.SuccessRate),
                TotalErrors: g.Sum(s => s.ErrorCount)
            ))
            .OrderBy(d => d.Date)
            .ToList();

        // Format breakdown
        var formatBreakdown = sessions
            .GroupBy(s => s.DetectedFormat)
            .Select(g => new FormatBreakdown(
                Format: g.Key,
                Count: g.Count(),
                Percentage: (double)g.Count() / sessions.Count * 100
            ))
            .OrderByDescending(f => f.Count)
            .ToList();

        // Outcome breakdown
        var outcomeBreakdown = sessions
            .GroupBy(s => s.Outcome)
            .Select(g => new OutcomeBreakdown(
                Outcome: g.Key.ToString(),
                Count: g.Count(),
                Percentage: (double)g.Count() / sessions.Count * 100,
                AverageSuccessRate: g.Average(s => s.SuccessRate)
            ))
            .OrderBy(o => (int)Enum.Parse<ParseOutcome>(o.Outcome))
            .ToList();

        var response = new AdminAnalyticsResponse(
            overview,
            dailyTrend,
            formatBreakdown,
            outcomeBreakdown
        );

        return Results.Ok(response);
    }
}
