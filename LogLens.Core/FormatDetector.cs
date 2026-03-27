using LogLens.Core.Parsers;

namespace LogLens.Core;

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

        foreach (var parser in _parsers.SkipLast(1))
            if (parser.CanParse(sample)) return parser;

        return _parsers.Last();
    }

    public string DetectFormatName(string rawLog) => Detect(rawLog).FormatName;
}
