using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

/// <summary>
/// New parser interface that returns structured log entries with display metadata.
/// Parsers should migrate from ILogParser to this interface.
/// </summary>
public interface IStructuredLogParser
{
    /// <summary>Format name (e.g., "Serilog", "NLog")</summary>
    string FormatName { get; }

    /// <summary>Determines if this parser can handle the given log sample</summary>
    bool CanParse(string sampleLines);

    /// <summary>Parses raw log into structured entries with field sections</summary>
    IEnumerable<StructuredLogEntry> ParseStructured(string rawLog);
}
