using System.Text.RegularExpressions;
using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

public abstract class BaseLogParser : ILogParser
{
    public abstract string FormatName { get; }
    public abstract bool CanParse(string sampleLines);
    public abstract IEnumerable<LogEntry> Parse(string rawLog);

    protected static LogLevel ParseLevel(string level) => level.ToUpperInvariant().Trim() switch
    {
        "TRACE" or "TRC" or "VERBOSE" or "VRB" => LogLevel.Trace,
        "DEBUG" or "DBG" => LogLevel.Debug,
        "INFO" or "INFORMATION" or "INF" => LogLevel.Info,
        "WARN" or "WARNING" or "WRN" => LogLevel.Warning,
        "ERROR" or "ERR" => LogLevel.Error,
        "FATAL" or "CRITICAL" or "FTL" or "CRIT" => LogLevel.Fatal,
        _ => LogLevel.Unknown
    };

    protected static string? ExtractExceptionType(string message)
    {
        var match = Regex.Match(message, @"([A-Z][a-zA-Z]+Exception)");
        return match.Success ? match.Value : null;
    }

    protected static (string body, string? stackTrace) SplitStackTrace(string message)
    {
        var idx = message.IndexOf("\n   at ", StringComparison.Ordinal);
        if (idx < 0) idx = message.IndexOf("\r\n   at ", StringComparison.Ordinal);
        if (idx < 0) return (message.Trim(), null);
        return (message[..idx].Trim(), message[idx..].Trim());
    }

    protected static IEnumerable<string[]> GroupMultilineEntries(
        string[] lines, Func<string, bool> isNewEntry)
    {
        var buffer = new List<string>();
        foreach (var line in lines)
        {
            if (isNewEntry(line) && buffer.Count > 0)
            {
                yield return buffer.ToArray();
                buffer.Clear();
            }
            buffer.Add(line);
        }
        if (buffer.Count > 0) yield return buffer.ToArray();
    }
}
