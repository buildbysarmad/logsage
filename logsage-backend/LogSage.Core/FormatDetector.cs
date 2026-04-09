using LogSage.Core.Parsers;

namespace LogSage.Core;

public class FormatDetector
{
    private readonly List<ILogParser> _parsers;

    public FormatDetector()
    {
        _parsers =
        [
            new SerilogFormatParser(),
            new NLogFormatParser(),
            new Log4NetFormatParser(),
            new StandardFormatParser(),
            new PlainTextParser()   // always last — fallback
        ];
    }

    public ILogParser Detect(string rawLog)
    {
        var sample = string.Join('\n', rawLog.Split('\n').Take(30));

        var parser = _parsers
            .SkipLast(1)
            .Where(p => p.CanParse(sample))
            .FirstOrDefault() ?? _parsers.Last();

        return parser;
    }

    public string DetectFormatName(string rawLog) => Detect(rawLog).FormatName;
}
