using LogSage.Core;

namespace LogSage.Api.Models.Responses;

public class AnalysisResponse
{
    public string DetectedFormat { get; set; }
    public int TotalLines { get; set; }
    public int ParsedEntries { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public int DebugCount { get; set; }
    public int ParseErrorCount { get; set; }
    public string? LogDuration { get; set; }
    public bool WasTruncated { get; set; }
    public List<ErrorGroupResponse> ErrorGroups { get; set; }
    public List<AiGroupAnalysis> AiAnalysis { get; set; }
    public string? SessionToken { get; set; }
    public ParseStatsResponse? ParseStats { get; set; }

    public AnalysisResponse(ParseResult result, List<AiGroupAnalysis> ai, bool wasTruncated)
    {
        DetectedFormat = result.DetectedFormat;
        TotalLines = result.TotalLines;
        ParsedEntries = result.ParsedEntries;
        ErrorCount = result.ErrorCount;
        WarningCount = result.WarningCount;
        InfoCount = result.InfoCount;
        DebugCount = result.DebugCount;
        ParseErrorCount = result.ParseErrorCount;
        LogDuration = result.LogDuration?.ToString(@"hh\:mm\:ss");
        WasTruncated = wasTruncated;
        AiAnalysis = ai;
        ErrorGroups = result.ErrorGroups.Select(g => new ErrorGroupResponse
        {
            GroupKey = g.GroupKey,
            RepresentativeMessage = g.RepresentativeMessage,
            Level = g.Level.ToString(),
            Count = g.Count,
            FirstSeen = g.FirstSeen?.ToString("o"),
            LastSeen = g.LastSeen?.ToString("o"),
            ExceptionType = g.ExceptionType,
            Source = g.Source,
            Entries = g.Entries.Take(20).Select(e => new LogEntryResponse
            {
                Timestamp = e.Timestamp?.ToString("o"),
                Level = e.Level.ToString(),
                Message = e.Message,
                StackTrace = e.StackTrace,
                Source = e.Source,
                LineNumber = e.LineNumber,
                RawLine = e.RawLine,
                ParserType = e.ParserType,
                FieldSections = e.FieldSections.Select(fs => new FieldSectionResponse
                {
                    SectionName = fs.SectionName,
                    DisplayOrder = fs.DisplayOrder,
                    Fields = fs.Fields.Select(f => new DisplayFieldResponse
                    {
                        Key = f.Key,
                        DisplayName = f.DisplayName,
                        Value = f.Value,
                        Type = f.Type.ToString(),
                        Importance = f.Importance.ToString(),
                        Hints = f.Hints
                    }).ToList()
                }).ToList(),
                // Legacy fields (backward compatibility)
                StructuredFields = e.StructuredFields,
                RequestId = e.RequestId,
                RequestPath = e.RequestPath,
                ConnectionId = e.ConnectionId,
                StatusCode = e.StatusCode,
                SourceContext = e.SourceContext,
                ParseError = e.ParseError,
                ParseErrorMessage = e.ParseErrorMessage
            }).ToList()
        }).ToList();
    }

    public AnalysisResponse(StructuredParseResult result, List<AiGroupAnalysis> ai, bool wasTruncated)
    {
        DetectedFormat = result.DetectedFormat;
        TotalLines = result.TotalLines;
        ParsedEntries = result.ParsedEntries;
        ErrorCount = result.ErrorCount;
        WarningCount = result.WarningCount;
        InfoCount = result.InfoCount;
        DebugCount = result.DebugCount;
        ParseErrorCount = result.ParseErrorCount;
        LogDuration = result.LogDuration?.ToString(@"hh\:mm\:ss");
        WasTruncated = wasTruncated;
        AiAnalysis = ai;
        ErrorGroups = result.ErrorGroups.Select(g => new ErrorGroupResponse
        {
            GroupKey = g.GroupKey,
            RepresentativeMessage = g.RepresentativeMessage,
            Level = g.Level.ToString(),
            Count = g.Count,
            FirstSeen = g.FirstSeen?.ToString("o"),
            LastSeen = g.LastSeen?.ToString("o"),
            ExceptionType = g.ExceptionType,
            Source = g.Source,
            Entries = g.Entries.Take(20).Select(e => new LogEntryResponse
            {
                Timestamp = e.Timestamp?.ToString("o"),
                Level = e.Level.ToString(),
                Message = e.Message,
                StackTrace = e.StackTrace,
                Source = e.Source,
                LineNumber = e.LineNumber,
                RawLine = e.RawLine,
                ParserType = e.ParserType,
                FieldSections = e.FieldSections.Select(fs => new FieldSectionResponse
                {
                    SectionName = fs.SectionName,
                    DisplayOrder = fs.DisplayOrder,
                    Fields = fs.Fields.Select(f => new DisplayFieldResponse
                    {
                        Key = f.Key,
                        DisplayName = f.DisplayName,
                        Value = f.Value,
                        Type = f.Type.ToString(),
                        Importance = f.Importance.ToString(),
                        Hints = f.Hints
                    }).ToList()
                }).ToList(),
                // Legacy fields (backward compatibility)
                RequestId = e.RequestId,
                RequestPath = e.RequestPath,
                ConnectionId = e.ConnectionId,
                StatusCode = e.StatusCode,
                SourceContext = e.SourceContext,
                ParseError = e.ParseError,
                ParseErrorMessage = e.ParseErrorMessage
            }).ToList()
        }).ToList();
    }
}

public class ErrorGroupResponse
{
    public string GroupKey { get; set; } = string.Empty;
    public string RepresentativeMessage { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? FirstSeen { get; set; }
    public string? LastSeen { get; set; }
    public string? ExceptionType { get; set; }
    public string? Source { get; set; }
    public List<LogEntryResponse> Entries { get; set; } = new();
}

public class LogEntryResponse
{
    public string? Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public int LineNumber { get; set; }
    public string RawLine { get; set; } = string.Empty;
    public string ParserType { get; set; } = string.Empty;

    // Structured field sections (new architecture)
    public List<FieldSectionResponse> FieldSections { get; set; } = [];

    // Legacy flat fields (backward compatibility - deprecated)
    public Dictionary<string, object>? StructuredFields { get; set; }
    public string? RequestId { get; set; }
    public string? RequestPath { get; set; }
    public string? ConnectionId { get; set; }
    public int? StatusCode { get; set; }
    public string? SourceContext { get; set; }

    // Parse metadata
    public bool ParseError { get; set; }
    public string? ParseErrorMessage { get; set; }
}

public class FieldSectionResponse
{
    public string SectionName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<DisplayFieldResponse> Fields { get; set; } = [];
}

public class DisplayFieldResponse
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Importance { get; set; } = string.Empty;
    public Dictionary<string, object>? Hints { get; set; }
}

public record ParseStatsResponse(
    int TotalEntries,
    int InfoCount,
    int WarningCount,
    int ErrorCount,
    int DebugCount,
    int ParseErrorCount,
    string DetectedFormat,
    int DurationMs
);
