using LogSage.Core;

namespace LogSage.Api.Models.Responses;

public class AnalysisResponse
{
    public string DetectedFormat { get; set; }
    public int TotalLines { get; set; }
    public int ParsedEntries { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public string? LogDuration { get; set; }
    public bool WasTruncated { get; set; }
    public List<ErrorGroupResponse> ErrorGroups { get; set; }
    public List<AiGroupAnalysis> AiAnalysis { get; set; }

    public AnalysisResponse(ParseResult result, List<AiGroupAnalysis> ai, bool wasTruncated)
    {
        DetectedFormat = result.DetectedFormat;
        TotalLines = result.TotalLines;
        ParsedEntries = result.ParsedEntries;
        ErrorCount = result.ErrorCount;
        WarningCount = result.WarningCount;
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
                RawLine = e.RawLine
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
}
