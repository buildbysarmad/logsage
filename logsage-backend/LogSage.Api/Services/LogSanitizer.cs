using System.Text;
using System.Text.RegularExpressions;

namespace LogSage.Api.Services;

public interface ILogSanitizer
{
    SanitizedInput Sanitize(string rawInput);
}

public record SanitizedInput(
    string Sample,      // first 50 lines, redacted
    int TotalLines,
    int TotalBytes
);

public class LogSanitizer : ILogSanitizer
{
    private static readonly Regex AuthorizationHeaderPattern = new(
        @"""Authorization""\s*:\s*""[^""]+""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BearerTokenPattern = new(
        @"Bearer\s+[A-Za-z0-9\-_\.]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ConnectionStringPattern = new(
        @"(Password|Pwd|User Id|User)=[^;,""'\s]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex IpAddressPattern = new(
        @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        RegexOptions.Compiled);

    public SanitizedInput Sanitize(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput))
            return new SanitizedInput(string.Empty, 0, 0);

        var totalBytes = Encoding.UTF8.GetByteCount(rawInput);
        var lines = rawInput.Split('\n');
        var totalLines = lines.Length;

        // Redact each line BEFORE slicing to 50
        var redactedLines = new List<string>(Math.Min(50, totalLines));
        for (var i = 0; i < Math.Min(50, totalLines); i++)
        {
            redactedLines.Add(RedactLine(lines[i]));
        }

        var sample = string.Join('\n', redactedLines);

        return new SanitizedInput(sample, totalLines, totalBytes);
    }

    private static string RedactLine(string line)
    {
        // Apply redactions in order specified
        var redacted = line;

        // 1. Authorization header values
        redacted = AuthorizationHeaderPattern.Replace(redacted, @"""Authorization"": ""[REDACTED]""");

        // 2. Bearer tokens anywhere
        redacted = BearerTokenPattern.Replace(redacted, "Bearer [REDACTED]");

        // 3. Connection string sensitive parts
        redacted = ConnectionStringPattern.Replace(redacted, "$1=[REDACTED]");

        // 4. Non-loopback IPv4 addresses
        redacted = IpAddressPattern.Replace(redacted, match =>
        {
            var ip = match.Value;
            // Skip loopback and private ranges
            if (ip.StartsWith("127.") ||
                ip.StartsWith("192.168.") ||
                ip.StartsWith("10.") ||
                ip.StartsWith("172.16.") ||
                ip.StartsWith("172.17.") ||
                ip.StartsWith("172.18.") ||
                ip.StartsWith("172.19.") ||
                ip.StartsWith("172.20.") ||
                ip.StartsWith("172.21.") ||
                ip.StartsWith("172.22.") ||
                ip.StartsWith("172.23.") ||
                ip.StartsWith("172.24.") ||
                ip.StartsWith("172.25.") ||
                ip.StartsWith("172.26.") ||
                ip.StartsWith("172.27.") ||
                ip.StartsWith("172.28.") ||
                ip.StartsWith("172.29.") ||
                ip.StartsWith("172.30.") ||
                ip.StartsWith("172.31."))
            {
                return ip;
            }
            return "[IP]";
        });

        return redacted;
    }
}
