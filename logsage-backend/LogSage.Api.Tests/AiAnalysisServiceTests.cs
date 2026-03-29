using LogSage.Api.Services;
using LogSage.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace LogSage.Api.Tests;

public class AiAnalysisServiceTests
{
    [Fact]
    public async Task AiAnalysisService_ReturnsEmptyList_WhenNoErrorGroups()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Anthropic:ApiKey"] = "test-key" })
            .Build();
        var httpFactory = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<AiAnalysisService>>();
        var service = new AiAnalysisService(config, httpFactory.Object, logger.Object);

        var emptyGroups = new List<ErrorGroup>();

        // Act
        var result = await service.AnalyzeGroupsAsync(emptyGroups);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AiAnalysisService_ReturnsGracefulFallback_WhenApiKeyInvalid()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Anthropic:ApiKey"] = "invalid-key" })
            .Build();

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\": \"Invalid API key\"}")
            });

        using var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = new Mock<ILogger<AiAnalysisService>>();
        var service = new AiAnalysisService(config, httpFactory.Object, logger.Object);

        var groups = new List<ErrorGroup>
        {
            new()
            {
                GroupKey = "test-key",
                ExceptionType = "TestException",
                RepresentativeMessage = "Test error",
                Count = 1,
                Level = LogSage.Core.Models.LogLevel.Error,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Entries = new List<LogEntry>
                {
                    new() { Level = LogSage.Core.Models.LogLevel.Error, Message = "Test error", RawLine = "[ERR] Test error" }
                }
            }
        };

        // Act
        var result = await service.AnalyzeGroupsAsync(groups);

        // Assert
        Assert.Single(result);
        Assert.Equal("test-key", result[0].GroupKey);
        Assert.Equal("MEDIUM", result[0].Severity);
        Assert.Contains("unavailable", result[0].RootCause, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AiAnalysisService_DoesNotAnalyzeWarnings_OnlyErrors()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Anthropic:ApiKey"] = "test-key" })
            .Build();
        var httpFactory = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<AiAnalysisService>>();
        var service = new AiAnalysisService(config, httpFactory.Object, logger.Object);

        var groups = new List<ErrorGroup>
        {
            new()
            {
                GroupKey = "warning-key",
                RepresentativeMessage = "Warning message",
                Count = 1,
                Level = LogSage.Core.Models.LogLevel.Warning,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Entries = new List<LogEntry>
                {
                    new() { Level = LogSage.Core.Models.LogLevel.Warning, Message = "Warning", RawLine = "[WRN] Warning" }
                }
            },
            new()
            {
                GroupKey = "info-key",
                RepresentativeMessage = "Info message",
                Count = 1,
                Level = LogSage.Core.Models.LogLevel.Info,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Entries = new List<LogEntry>
                {
                    new() { Level = LogSage.Core.Models.LogLevel.Info, Message = "Info", RawLine = "[INF] Info" }
                }
            }
        };

        // Act
        var result = await service.AnalyzeGroupsAsync(groups);

        // Assert
        Assert.Empty(result); // Only Error and Fatal levels should be analyzed
    }
}
