using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Core;
using LogSage.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Services;

public class SessionService(AppDbContext db)
{
    private const int FreeTierDailyLimit = 3;

    public async Task<bool> IsWithinFreeTierLimitAsync(
        string identifier, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var usage = await db.UsageTracking
            .FirstOrDefaultAsync(u => u.Identifier == identifier && u.Date == today, ct);
        return usage == null || usage.SessionCount < FreeTierDailyLimit;
    }

    public async Task IncrementUsageAsync(
        string identifier, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Atomic increment using ExecuteUpdate to prevent race conditions
        var updated = await db.UsageTracking
            .Where(u => u.Identifier == identifier && u.Date == today)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.SessionCount, x => x.SessionCount + 1), ct);

        // If no rows updated, insert new record
        if (updated == 0)
        {
            try
            {
                db.UsageTracking.Add(new UsageTracking {
                    Identifier = identifier, Date = today, SessionCount = 1 });
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Race condition: another request inserted first. Retry increment.
                await db.UsageTracking
                    .Where(u => u.Identifier == identifier && u.Date == today)
                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.SessionCount, x => x.SessionCount + 1), ct);
            }
        }
    }

    /// <summary>
    /// Saves an analysis session to the database
    /// </summary>
    /// <param name="userId">User ID if authenticated, null for anonymous</param>
    /// <param name="result">Analysis result from LogSageEngine</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The saved session ID</returns>
    public async Task<Guid> SaveSessionAsync(
        Guid? userId, ParseResult result, CancellationToken ct = default)
    {
        var session = new Session
        {
            UserId = userId,
            DetectedFormat = result.DetectedFormat,
            TotalLines = result.TotalLines,
            ErrorCount = result.ErrorGroups.Count(g => g.Level == Core.Models.LogLevel.Error || g.Level == Core.Models.LogLevel.Fatal),
            WarningCount = result.ErrorGroups.Count(g => g.Level == Core.Models.LogLevel.Warning),
            CreatedAt = DateTime.UtcNow
        };

        // Map error groups
        foreach (var group in result.ErrorGroups)
        {
            session.ErrorGroups.Add(new ErrorGroupEntity
            {
                GroupKey = group.GroupKey,
                RepresentativeMessage = group.RepresentativeMessage,
                Level = group.Level.ToString(),
                Count = group.Count,
                ExceptionType = group.ExceptionType,
                FirstSeen = group.FirstSeen,
                LastSeen = group.LastSeen
            });
        }

        db.Sessions.Add(session);
        await db.SaveChangesAsync(ct);

        return session.Id;
    }
}
