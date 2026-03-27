using LogLens.Api.Data;
using LogLens.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogLens.Api.Services;

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
        var usage = await db.UsageTracking
            .FirstOrDefaultAsync(u => u.Identifier == identifier && u.Date == today, ct);

        if (usage == null)
            db.UsageTracking.Add(new UsageTracking {
                Identifier = identifier, Date = today, SessionCount = 1 });
        else
            usage.SessionCount++;

        await db.SaveChangesAsync(ct);
    }
}
