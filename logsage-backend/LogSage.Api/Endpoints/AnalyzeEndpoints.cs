using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Data.Repositories;
using LogSage.Api.Infrastructure;
using LogSage.Api.Models.Requests;
using LogSage.Api.Models.Responses;
using LogSage.Api.Services;
using LogSage.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

        app.MapPost("/api/sessions/{sessionToken}/feedback", SubmitFeedback)
           .WithTags("Sessions")
           .WithSummary("Submit feedback for anonymous session — no auth required")
           .WithDescription("Rate limited to 10 requests per IP per hour.");

        app.MapGet("/api/admin/sessions", GetAdminSessions)
           .WithTags("Admin")
           .WithSummary("Get anonymous session analytics — admin key required")
           .WithDescription("Paginated list of all anonymous parse sessions with filtering.")
           .AddEndpointFilter<AdminKeyFilter>();
    }

    private static async Task<IResult> AnalyzeText(
        AnalyzeRequest req, LogSageEngine engine,
        AiAnalysisService ai, SessionService sessions,
        ILogSanitizer sanitizer, IParseSessionRepository parseSessions,
        HttpContext ctx, IConfiguration config, ILogger<AnalyzeRequest> logger, CancellationToken ct)
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

        // Sanitize input for observability
        var sanitized = sanitizer.Sanitize(req.RawLog);
        var sessionToken = GenerateSessionToken();

        var wasTruncated = pricingEnabled && !isPro && lines.Length > 500;
        var capped = wasTruncated ? string.Join('\n', lines.Take(500)) : req.RawLog;

        // Start timing and parse
        var stopwatch = Stopwatch.StartNew();
        var result = engine.AnalyzeStructured(capped);
        stopwatch.Stop();

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

        // Persist anonymous parse session for observability
        await PersistParseSessionAsync(parseSessions, sessionToken, sanitized, result,
            (int)stopwatch.ElapsedMilliseconds, logger, ct);

        // Add session token to response header
        ctx.Response.Headers["X-Session-Token"] = sessionToken;

        var response = new AnalysisResponse(result, aiResults, wasTruncated)
        {
            SessionToken = sessionToken,
            ParseStats = new ParseStatsResponse(
                result.ParsedEntries,
                result.InfoCount,
                result.WarningCount,
                result.ErrorCount,
                result.DebugCount,
                result.ParseErrorCount,
                result.DetectedFormat,
                (int)stopwatch.ElapsedMilliseconds
            )
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> AnalyzeFile(
        IFormFile file, LogSageEngine engine,
        AiAnalysisService ai, SessionService sessions,
        ILogSanitizer sanitizer, IParseSessionRepository parseSessions,
        HttpContext ctx, IConfiguration config, ILogger<AnalyzeRequest> logger, CancellationToken ct)
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

        // Read file content for sanitization and parsing
        string rawContent;
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            rawContent = await reader.ReadToEndAsync(ct);
        }

        // Sanitize input for observability
        var sanitized = sanitizer.Sanitize(rawContent);
        var sessionToken = GenerateSessionToken();

        // Start timing and parse
        var stopwatch = Stopwatch.StartNew();
        var result = engine.AnalyzeStructured(rawContent);
        stopwatch.Stop();

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

        // Persist anonymous parse session for observability
        await PersistParseSessionAsync(parseSessions, sessionToken, sanitized, result,
            (int)stopwatch.ElapsedMilliseconds, logger, ct);

        // Add session token to response header
        ctx.Response.Headers["X-Session-Token"] = sessionToken;

        var response = new AnalysisResponse(result, aiResults, false)
        {
            SessionToken = sessionToken,
            ParseStats = new ParseStatsResponse(
                result.ParsedEntries,
                result.InfoCount,
                result.WarningCount,
                result.ErrorCount,
                result.DebugCount,
                result.ParseErrorCount,
                result.DetectedFormat,
                (int)stopwatch.ElapsedMilliseconds
            )
        };

        return Results.Ok(response);
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

    private static async Task<IResult> SubmitFeedback(
        string sessionToken, FeedbackRequest req, IParseSessionRepository repo,
        IMemoryCache cache, HttpContext ctx, ILogger<FeedbackRequest> logger, CancellationToken ct)
    {
        // Validate score
        if (req.Score != 1 && req.Score != -1)
            return Results.BadRequest(new { error = "Score must be 1 (thumbs up) or -1 (thumbs down)" });

        // Check session exists
        var session = await repo.GetByTokenAsync(sessionToken, ct);
        if (session == null)
            return Results.NotFound(new { error = "Session not found" });

        // Check if feedback already exists
        if (session.FeedbackScore.HasValue)
            return Results.Conflict(new { error = "Feedback already submitted for this session" });

        // Rate limiting: 10 requests per IP per hour
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cacheKey = $"feedback_ratelimit_{ip}";
        if (!cache.TryGetValue<List<DateTime>>(cacheKey, out var requests))
        {
            requests = [];
        }

        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);
        requests = requests!.Where(r => r > oneHourAgo).ToList();

        if (requests.Count >= 10)
            return Results.StatusCode(429);

        requests.Add(now);
        cache.Set(cacheKey, requests, TimeSpan.FromHours(1));

        // Update feedback
        await repo.UpdateFeedbackAsync(sessionToken, req.Score, ct);

        logger.LogInformation("Feedback received {SessionId} Score={Score}", session.Id, req.Score);

        return Results.NoContent();
    }

    private static async Task<IResult> GetAdminSessions(
        IParseSessionRepository repo, HttpContext ctx,
        int page = 1, int pageSize = 50,
        bool? parseSuccess = null, bool? hasErrors = null, bool? hasFeedback = null,
        CancellationToken ct = default)
    {
        // Validate pagination
        if (page < 1)
            return Results.BadRequest(new { error = "Page must be >= 1" });

        if (pageSize < 1 || pageSize > 100)
            return Results.BadRequest(new { error = "Page size must be between 1 and 100" });

        var filter = new ParseSessionFilter(parseSuccess, hasErrors, hasFeedback);
        var (items, total) = await repo.GetPagedAsync(filter, page, pageSize, ct);

        var response = new
        {
            total,
            page,
            pageSize,
            items
        };

        return Results.Ok(response);
    }

    private static string GenerateSessionToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var base64 = Convert.ToBase64String(bytes);
        // Make URL-safe
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static async Task PersistParseSessionAsync(
        IParseSessionRepository repo, string sessionToken, SanitizedInput sanitized,
        ParseResult result, int durationMs, ILogger logger, CancellationToken ct)
    {
        try
        {
            // Get up to 3 parse error samples
            var errorSamples = result.UnparsedLines.Take(3).ToList();
            var errorSamplesJson = errorSamples.Count > 0
                ? JsonSerializer.Serialize(errorSamples)
                : null;

            var parseSession = new ParseSession
            {
                SessionToken = sessionToken,
                InputSample = sanitized.Sample,
                InputLineCount = sanitized.TotalLines,
                InputSizeBytes = sanitized.TotalBytes,
                DetectedFormat = result.DetectedFormat,
                ParseSuccess = result.ParseErrorCount == 0,
                TotalEntries = result.ParsedEntries,
                InfoCount = result.InfoCount,
                WarningCount = result.WarningCount,
                ErrorCount = result.ErrorCount,
                DebugCount = result.DebugCount,
                ParseErrorCount = result.ParseErrorCount,
                ParseErrorSamples = errorSamplesJson,
                DurationMs = durationMs
            };

            await repo.CreateAsync(parseSession, ct);

            logger.LogInformation(
                "ParseSession created {SessionId} Format={DetectedFormat} Entries={TotalEntries} ParseSuccess={ParseSuccess} DurationMs={DurationMs}",
                parseSession.Id, parseSession.DetectedFormat, parseSession.TotalEntries,
                parseSession.ParseSuccess, parseSession.DurationMs);
        }
        catch (Exception ex)
        {
            // Log but don't fail the request — analysis result is more important than observability
            logger.LogError(ex, "Failed to persist parse session for observability");
        }
    }

    private static async Task PersistParseSessionAsync(
        IParseSessionRepository repo, string sessionToken, SanitizedInput sanitized,
        StructuredParseResult result, int durationMs, ILogger logger, CancellationToken ct)
    {
        try
        {
            // Get up to 3 parse error samples
            var errorSamples = result.UnparsedLines.Take(3).ToList();
            var errorSamplesJson = errorSamples.Count > 0
                ? JsonSerializer.Serialize(errorSamples)
                : null;

            var parseSession = new ParseSession
            {
                SessionToken = sessionToken,
                InputSample = sanitized.Sample,
                InputLineCount = sanitized.TotalLines,
                InputSizeBytes = sanitized.TotalBytes,
                DetectedFormat = result.DetectedFormat,
                ParseSuccess = result.ParseErrorCount == 0,
                TotalEntries = result.ParsedEntries,
                InfoCount = result.InfoCount,
                WarningCount = result.WarningCount,
                ErrorCount = result.ErrorCount,
                DebugCount = result.DebugCount,
                ParseErrorCount = result.ParseErrorCount,
                ParseErrorSamples = errorSamplesJson,
                DurationMs = durationMs
            };

            await repo.CreateAsync(parseSession, ct);

            logger.LogInformation(
                "ParseSession created {SessionId} Format={DetectedFormat} Entries={TotalEntries} ParseSuccess={ParseSuccess} DurationMs={DurationMs}",
                parseSession.Id, parseSession.DetectedFormat, parseSession.TotalEntries,
                parseSession.ParseSuccess, parseSession.DurationMs);
        }
        catch (Exception ex)
        {
            // Log but don't fail the request — analysis result is more important than observability
            logger.LogError(ex, "Failed to persist parse session for observability");
        }
    }
}
