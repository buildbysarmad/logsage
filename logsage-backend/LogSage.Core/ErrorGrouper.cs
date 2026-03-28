using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core;

public class ErrorGrouper
{
    private static readonly LogLevel[] GroupedLevels =
        [LogLevel.Error, LogLevel.Fatal, LogLevel.Warning];

    public List<ErrorGroup> Group(IEnumerable<LogEntry> entries)
    {
        var groups = new Dictionary<string, ErrorGroup>();

        foreach (var entry in entries.Where(e => GroupedLevels.Contains(e.Level)))
        {
            var key = BuildGroupKey(entry);

            if (!groups.TryGetValue(key, out var group))
            {
                group = new ErrorGroup
                {
                    GroupKey = key,
                    RepresentativeMessage = entry.Message,
                    Level = entry.Level,
                    ExceptionType = entry.ExceptionType,
                    Source = entry.Source,
                    FirstSeen = entry.Timestamp,
                    LastSeen = entry.Timestamp
                };
                groups[key] = group;
            }

            group.Entries.Add(entry);
            group.Count++;

            if (entry.Timestamp.HasValue)
            {
                if (!group.FirstSeen.HasValue || entry.Timestamp < group.FirstSeen)
                    group.FirstSeen = entry.Timestamp;
                if (!group.LastSeen.HasValue || entry.Timestamp > group.LastSeen)
                    group.LastSeen = entry.Timestamp;
            }
        }

        return groups.Values
            .OrderByDescending(g => g.Level)
            .ThenByDescending(g => g.Count)
            .ToList();
    }

    private static string BuildGroupKey(LogEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.ExceptionType))
        {
            var location = ExtractTopFrame(entry.StackTrace);
            return location != null
                ? $"{entry.ExceptionType}::{location}"
                : entry.ExceptionType;
        }

        return $"{entry.Level}::{NormalizeMessage(entry.Message)}";
    }

    private static string NormalizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;

        var n = message;
        n = Regex.Replace(n, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", "{guid}");
        n = Regex.Replace(n, @"\b\d{3,}\b", "{id}");
        n = Regex.Replace(n, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "{ip}");
        n = Regex.Replace(n, @"[A-Za-z]:\\[^\s]+|\/[^\s]+\/[^\s]+", "{path}");
        n = Regex.Replace(n, @"'[^']{1,50}'", "'{val}'");
        n = Regex.Replace(n, @"""[^""]{1,50}""", "\"{val}\"");
        return n.Trim().ToLowerInvariant()[..Math.Min(n.Length, 120)];
    }

    private static string? ExtractTopFrame(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace)) return null;
        var match = Regex.Match(stackTrace, @"at\s+([A-Za-z0-9_.]+)\(");
        return match.Success ? match.Groups[1].Value : null;
    }
}
