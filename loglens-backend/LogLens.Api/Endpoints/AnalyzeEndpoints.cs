using LogLens.Api.Data;
using LogLens.Api.Models.Requests;
using LogLens.Api.Models.Responses;
using LogLens.Api.Services;
using LogLens.Core;
using Microsoft.EntityFrameworkCore;

namespace LogLens.Api.Endpoints;

public static class AnalyzeEndpoints
{
    public static void MapAnalyzeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/analyze", AnalyzeText);
        app.MapPost("/api/analyze/upload", AnalyzeFile).DisableAntiforgery();
        app.MapGet("/api/sessions", GetSessions).RequireAuthorization();
        app.MapGet("/api/sessions/{id:guid}", GetSession).RequireAuthorization();
    }

    private static async Task<IResult> AnalyzeText(
        AnalyzeRequest req, LogLensEngine engine,
        AiAnalysisService ai, SessionService sessions,
        HttpContext ctx, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RawLog))
            return Results.BadRequest(new { error = "rawLog is required" });

        var identifier = ctx.User.Identity?.Name
            ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
        var isPro = ctx.User.HasClaim("plan", "pro") ||
                    ctx.User.HasClaim("plan", "team");

        if (!isPro && !await sessions.IsWithinFreeTierLimitAsync(identifier, ct))
            return Results.StatusCode(429);

        var lines = req.RawLog.Split('\n');
        var wasTruncated = !isPro && lines.Length > 500;
        var capped = wasTruncated ? string.Join('\n', lines.Take(500)) : req.RawLog;
        var result = engine.Analyze(capped);

        List<AiGroupAnalysis> aiResults = [];
        if (isPro) aiResults = await ai.AnalyzeGroupsAsync(result.ErrorGroups, ct);
        await sessions.IncrementUsageAsync(identifier, ct);

        return Results.Ok(new AnalysisResponse(result, aiResults, wasTruncated));
    }

    private static async Task<IResult> AnalyzeFile(
        IFormFile file, LogLensEngine engine,
        AiAnalysisService ai, SessionService sessions,
        HttpContext ctx, CancellationToken ct)
    {
        if (file.Length > 10 * 1024 * 1024)
            return Results.BadRequest(new { error = "File too large (max 10MB)" });

        var identifier = ctx.User.Identity?.Name
            ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
        var isPro = ctx.User.HasClaim("plan", "pro") ||
                    ctx.User.HasClaim("plan", "team");

        if (!isPro && !await sessions.IsWithinFreeTierLimitAsync(identifier, ct))
            return Results.StatusCode(429);

        using var stream = file.OpenReadStream();
        var result = await engine.AnalyzeStreamAsync(stream);

        List<AiGroupAnalysis> aiResults = [];
        if (isPro) aiResults = await ai.AnalyzeGroupsAsync(result.ErrorGroups, ct);
        await sessions.IncrementUsageAsync(identifier, ct);

        return Results.Ok(new AnalysisResponse(result, aiResults, false));
    }

    private static async Task<IResult> GetSessions(
        AppDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userId = Guid.Parse(ctx.User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var list = await db.Sessions
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
            .Include(s => s.ErrorGroups)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);
        return session == null ? Results.NotFound() : Results.Ok(session);
    }
}
