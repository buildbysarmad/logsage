using LogSage.Api.Data;
using LogSage.Api.Models.Requests;
using LogSage.Api.Models.Responses;
using LogSage.Api.Services;
using LogSage.Core;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Endpoints;

public static class AnalyzeEndpoints
{
    public static void MapAnalyzeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/analyze", AnalyzeText)
           .WithTags("Analyze")
           .WithSummary("Analyze log text — no auth required")
           .WithDescription("5,000 line limit. AI analysis available for Pro users when AI_ENABLED is true.");

        app.MapPost("/api/analyze/upload", AnalyzeFile)
           .WithTags("Analyze")
           .WithSummary("Analyze log file upload — no auth required")
           .WithDescription("2MB file size limit, 5,000 line limit. AI analysis available for Pro users when AI_ENABLED is true.")
           .DisableAntiforgery();

        app.MapGet("/api/sessions", GetSessions)
           .WithTags("Sessions")
           .WithSummary("Get session history — auth required")
           .RequireAuthorization();

        app.MapGet("/api/sessions/{id:guid}", GetSession)
           .WithTags("Sessions")
           .WithSummary("Get single session by ID — auth required")
           .RequireAuthorization();
    }

    private static async Task<IResult> AnalyzeText(
        AnalyzeRequest req, LogSageEngine engine,
        AiAnalysisService ai, SessionService sessions,
        HttpContext ctx, IConfiguration config, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RawLog))
            return Results.BadRequest(new { error = "rawLog is required" });

        var identifier = ctx.User.Identity?.Name
            ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
        var isPro = ctx.User.HasClaim("plan", "pro") ||
                    ctx.User.HasClaim("plan", "team");

        // Only enforce free tier limits when pricing is enabled
        var pricingEnabled = config.GetValue<bool>("PRICING_ENABLED");
        if (pricingEnabled && !isPro && !await sessions.IsWithinFreeTierLimitAsync(identifier, ct))
            return Results.StatusCode(429);

        var lines = req.RawLog.Split('\n');

        // Enforce 5,000 line limit for all users BEFORE parsing
        if (lines.Length > 5000)
            return Results.BadRequest(new { error = "Log exceeds the 5,000 line limit." });

        var wasTruncated = pricingEnabled && !isPro && lines.Length > 500;
        var capped = wasTruncated ? string.Join('\n', lines.Take(500)) : req.RawLog;
        var result = engine.Analyze(capped);

        List<AiGroupAnalysis> aiResults = [];
        var aiEnabled = config.GetValue<bool>("AI_ENABLED");
        if (isPro && aiEnabled)
        {
            aiResults = await ai.AnalyzeGroupsAsync(result.ErrorGroups, ct);
        }

        if (pricingEnabled)
            await sessions.IncrementUsageAsync(identifier, ct);

        // Save session to database (for authenticated users only)
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = ctx.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                await sessions.SaveSessionAsync(userId, result, ct);
            }
        }

        return Results.Ok(new AnalysisResponse(result, aiResults, wasTruncated));
    }

    private static async Task<IResult> AnalyzeFile(
        IFormFile file, LogSageEngine engine,
        AiAnalysisService ai, SessionService sessions,
        HttpContext ctx, IConfiguration config, CancellationToken ct)
    {
        if (file.Length > 2 * 1024 * 1024)
            return Results.BadRequest(new { error = "File too large (max 2MB)" });

        var identifier = ctx.User.Identity?.Name
            ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
        var isPro = ctx.User.HasClaim("plan", "pro") ||
                    ctx.User.HasClaim("plan", "team");

        // Only enforce free tier limits when pricing is enabled
        var pricingEnabled = config.GetValue<bool>("PRICING_ENABLED");
        if (pricingEnabled && !isPro && !await sessions.IsWithinFreeTierLimitAsync(identifier, ct))
            return Results.StatusCode(429);

        using var stream = file.OpenReadStream();
        var result = await engine.AnalyzeStreamAsync(stream);

        // Enforce 5,000 line limit for all users
        if (result.TotalLines > 5000)
            return Results.BadRequest(new { error = "Log exceeds the 5,000 line limit." });

        List<AiGroupAnalysis> aiResults = [];
        var aiEnabled = config.GetValue<bool>("AI_ENABLED");
        if (isPro && aiEnabled)
        {
            aiResults = await ai.AnalyzeGroupsAsync(result.ErrorGroups, ct);
        }

        if (pricingEnabled)
            await sessions.IncrementUsageAsync(identifier, ct);

        // Save session to database (for authenticated users only)
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = ctx.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                await sessions.SaveSessionAsync(userId, result, ct);
            }
        }

        return Results.Ok(new AnalysisResponse(result, aiResults, false));
    }

    private static async Task<IResult> GetSessions(
        AppDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userId = Guid.Parse(ctx.User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var list = await db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .Select(s => new {
                s.Id, s.DetectedFormat, s.TotalLines,
                s.ErrorCount, s.WarningCount, s.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetSession(
        Guid id, AppDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userId = Guid.Parse(ctx.User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var session = await db.Sessions
            .AsNoTracking()
            .Include(s => s.ErrorGroups)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);
        return session == null ? Results.NotFound() : Results.Ok(session);
    }
}
