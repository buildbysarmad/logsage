using System.Text.Json;
using LogSage.Api.Models.Responses;
using LogSage.Core.Models;

namespace LogSage.Api.Services;

public class AiAnalysisService(
    IConfiguration config,
    IHttpClientFactory http,
    ILogger<AiAnalysisService> logger)
{
    private const string Model = "claude-sonnet-4-20250514";
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public async Task<List<AiGroupAnalysis>> AnalyzeGroupsAsync(
        List<ErrorGroup> groups, CancellationToken ct = default)
    {
        var relevant = groups
            .Where(g => g.Level is LogSage.Core.Models.LogLevel.Error or LogSage.Core.Models.LogLevel.Fatal)
            .Take(10).ToList();
        if (!relevant.Any()) return [];
        var tasks = relevant.Select(g => AnalyzeGroupAsync(g, ct));
        return (await Task.WhenAll(tasks)).ToList();
    }

    private async Task<AiGroupAnalysis> AnalyzeGroupAsync(
        ErrorGroup group, CancellationToken ct)
    {
        try
        {
            var client = http.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", config["Anthropic:ApiKey"]);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var samples = group.Entries.Take(3).Select(e => e.RawLine);
            var prompt = $$"""
                You are a senior software engineer analyzing application logs.
                Respond in JSON only — no preamble, no markdown.

                Error type: {{group.ExceptionType ?? "Unknown"}}
                Occurrences: {{group.Count}}
                First seen: {{group.FirstSeen}} | Last seen: {{group.LastSeen}}
                Message: {{group.RepresentativeMessage}}
                Samples:
                {{string.Join('\n', samples)}}
                Stack trace: {{group.Entries.FirstOrDefault()?.StackTrace ?? "none"}}

                Respond ONLY with:
                {
                  "severity": "LOW|MEDIUM|HIGH|CRITICAL",
                  "rootCause": "1-2 sentence explanation",
                  "suggestedFix": "Concrete steps to fix (max 5)"
                }
                """;

            var response = await client.PostAsJsonAsync(ApiUrl, new {
                model = Model, max_tokens = 400,
                messages = new[] { new { role = "user", content = prompt } }
            }, ct);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            var json = result.GetProperty("content")[0].GetProperty("text").GetString() ?? "{}";

            var analysis = JsonSerializer.Deserialize<AiGroupAnalysis>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            analysis.GroupKey = group.GroupKey;
            return analysis;
        }
        catch (OperationCanceledException)
        {
            // Preserve cancellation semantics
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error while analyzing group {GroupKey}", group.GroupKey);
            return new AiGroupAnalysis
            {
                GroupKey = group.GroupKey,
                Severity = "MEDIUM",
                RootCause = "AI analysis unavailable due to HTTP error.",
                SuggestedFix = "Review the stack trace and HTTP connectivity manually."
            };
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse AI analysis response for group {GroupKey}", group.GroupKey);
            return new AiGroupAnalysis
            {
                GroupKey = group.GroupKey,
                Severity = "MEDIUM",
                RootCause = "AI analysis unavailable due to invalid response.",
                SuggestedFix = "Review the stack trace and raw AI response manually."
            };
        }
    }
}
