using LogSage.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Data.Repositories;

public interface IParseSessionRepository
{
    Task<ParseSession> CreateAsync(ParseSession session, CancellationToken ct = default);
    Task<ParseSession?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task UpdateFeedbackAsync(string token, int score, string? note, CancellationToken ct = default);
    Task<(List<ParseSessionSummary> Items, int Total)> GetPagedAsync(
        ParseSessionFilter filter, int page, int pageSize, CancellationToken ct = default);
}

public record ParseSessionFilter(
    bool? ParseSuccess,
    bool? HasErrors,
    bool? HasFeedback
);

public record ParseSessionSummary(
    Guid Id,
    DateTime CreatedAt,
    string DetectedFormat,
    bool ParseSuccess,
    int TotalEntries,
    int InfoCount,
    int WarningCount,
    int ErrorCount,
    int ParseErrorCount,
    int? FeedbackScore,
    string? FeedbackNote,
    int InputSizeBytes,
    int InputLineCount,
    int DurationMs
);

public class ParseSessionRepository(AppDbContext db) : IParseSessionRepository
{
    public async Task<ParseSession> CreateAsync(ParseSession session, CancellationToken ct = default)
    {
        db.ParseSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<ParseSession?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await db.ParseSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionToken == token, ct);
    }

    public async Task UpdateFeedbackAsync(string token, int score, string? note, CancellationToken ct = default)
    {
        var session = await db.ParseSessions
            .FirstOrDefaultAsync(s => s.SessionToken == token, ct);

        if (session == null)
            return;

        session.FeedbackScore = score;
        session.FeedbackNote = note;
        session.FeedbackAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task<(List<ParseSessionSummary> Items, int Total)> GetPagedAsync(
        ParseSessionFilter filter, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.ParseSessions.AsNoTracking();

        // Apply filters
        if (filter.ParseSuccess.HasValue)
            query = query.Where(s => s.ParseSuccess == filter.ParseSuccess.Value);

        if (filter.HasErrors.HasValue)
        {
            if (filter.HasErrors.Value)
                query = query.Where(s => s.ErrorCount > 0);
            else
                query = query.Where(s => s.ErrorCount == 0);
        }

        if (filter.HasFeedback.HasValue)
        {
            if (filter.HasFeedback.Value)
                query = query.Where(s => s.FeedbackScore != null);
            else
                query = query.Where(s => s.FeedbackScore == null);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ParseSessionSummary(
                s.Id,
                s.CreatedAt,
                s.DetectedFormat,
                s.ParseSuccess,
                s.TotalEntries,
                s.InfoCount,
                s.WarningCount,
                s.ErrorCount,
                s.ParseErrorCount,
                s.FeedbackScore,
                s.FeedbackNote,
                s.InputSizeBytes,
                s.InputLineCount,
                s.DurationMs
            ))
            .ToListAsync(ct);

        return (items, total);
    }
}
