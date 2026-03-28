using LogSage.Core.Models;

namespace LogSage.Core.Parsers;

public interface ILogParser
{
    string FormatName { get; }
    bool CanParse(string sampleLines);
    IEnumerable<LogEntry> Parse(string rawLog);
}
