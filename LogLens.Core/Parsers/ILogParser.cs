using LogLens.Core.Models;

namespace LogLens.Core.Parsers;

public interface ILogParser
{
    string FormatName { get; }
    bool CanParse(string sampleLines);
    IEnumerable<LogEntry> Parse(string rawLog);
}
