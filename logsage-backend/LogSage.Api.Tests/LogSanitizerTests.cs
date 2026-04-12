using LogSage.Api.Services;

namespace LogSage.Api.Tests;

public class LogSanitizerTests
{
    private readonly ILogSanitizer _sanitizer = new LogSanitizer();

    [Fact]
    public void Sanitize_BearerTokenInJsonPayload_IsRedacted()
    {
        // Arrange
        var input = @"{""token"": ""Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test"", ""data"": ""value""}";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Contains("Bearer [REDACTED]", result.Sample);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", result.Sample);
    }

    [Fact]
    public void Sanitize_AuthorizationHeaderValue_IsRedacted()
    {
        // Arrange
        var input = @"POST /api/test HTTP/1.1
Host: example.com
""Authorization"": ""Bearer secret-token-here""
Content-Type: application/json";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Contains(@"""Authorization"": ""[REDACTED]""", result.Sample);
        Assert.DoesNotContain("secret-token-here", result.Sample);
    }

    [Fact]
    public void Sanitize_LoopbackIps_AreNotRedacted()
    {
        // Arrange
        var input = @"Connection from 127.0.0.1:8080
Local address: 127.0.0.1
IPv6 loopback: ::1
Private network: 192.168.1.100
Another private: 10.0.0.5";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Contains("127.0.0.1", result.Sample);
        Assert.Contains("192.168.1.100", result.Sample);
        Assert.Contains("10.0.0.5", result.Sample);
    }

    [Fact]
    public void Sanitize_PublicIp_IsRedacted()
    {
        // Arrange
        var input = @"Request from 203.0.113.45:54321
Connection to 8.8.8.8 established
Internal IP: 192.168.1.1";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Contains("[IP]", result.Sample);
        Assert.DoesNotContain("203.0.113.45", result.Sample);
        Assert.DoesNotContain("8.8.8.8", result.Sample);
        // Private IPs should not be redacted
        Assert.Contains("192.168.1.1", result.Sample);
    }

    [Fact]
    public void Sanitize_InputWith200Lines_ProducesSampleWith50Lines()
    {
        // Arrange
        var lines = Enumerable.Range(1, 200).Select(i => $"Line {i}");
        var input = string.Join('\n', lines);

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Equal(200, result.TotalLines);
        var sampleLines = result.Sample.Split('\n');
        Assert.Equal(50, sampleLines.Length);
        Assert.Equal("Line 1", sampleLines[0]);
        Assert.Equal("Line 50", sampleLines[49]);
    }

    [Fact]
    public void Sanitize_EmptyInput_ReturnsEmptySample()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Equal(string.Empty, result.Sample);
        Assert.Equal(0, result.TotalLines);
        Assert.Equal(0, result.TotalBytes);
    }

    [Fact]
    public void Sanitize_ConnectionStringPassword_IsRedacted()
    {
        // Arrange
        var input = @"Server=localhost;Database=mydb;User Id=admin;Password=secret123;";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Contains("Password=[REDACTED]", result.Sample);
        Assert.Contains("User Id=[REDACTED]", result.Sample);
        Assert.DoesNotContain("secret123", result.Sample);
        Assert.DoesNotContain("admin", result.Sample);
    }

    [Fact]
    public void Sanitize_TotalBytesAndLines_AreCorrect()
    {
        // Arrange
        var input = "Line 1\nLine 2\nLine 3";
        var expectedBytes = System.Text.Encoding.UTF8.GetByteCount(input);

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Equal(3, result.TotalLines);
        Assert.Equal(expectedBytes, result.TotalBytes);
    }

    [Fact]
    public void Sanitize_RedactionAppliesBeforeSlicing()
    {
        // Arrange
        var lines = new List<string>();
        for (var i = 1; i <= 100; i++)
        {
            lines.Add($"Line {i} with Bearer secret-token-{i}");
        }
        var input = string.Join('\n', lines);

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        var sampleLines = result.Sample.Split('\n');
        Assert.Equal(50, sampleLines.Length);
        // All 50 lines should have redacted tokens
        foreach (var line in sampleLines)
        {
            Assert.Contains("Bearer [REDACTED]", line);
            Assert.DoesNotContain("secret-token", line);
        }
    }
}
