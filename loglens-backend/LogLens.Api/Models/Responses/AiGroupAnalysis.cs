using System.Text.Json.Serialization;

namespace LogLens.Api.Models.Responses;

public class AiGroupAnalysis
{
    [JsonPropertyName("groupKey")]
    public string GroupKey { get; set; } = string.Empty;
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "MEDIUM";
    [JsonPropertyName("rootCause")]
    public string RootCause { get; set; } = string.Empty;
    [JsonPropertyName("suggestedFix")]
    public string SuggestedFix { get; set; } = string.Empty;
}
