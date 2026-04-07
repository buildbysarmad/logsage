using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
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
}
