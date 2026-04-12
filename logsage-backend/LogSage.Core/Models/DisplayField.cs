namespace LogSage.Core.Models;

/// <summary>
/// Represents a single field to be displayed, with type and rendering hints
/// </summary>
public class DisplayField
{
    /// <summary>Internal field key (e.g., "requestId", "statusCode")</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable display name (e.g., "Request ID", "HTTP Status")</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Field value (can be string, number, etc.)</summary>
    public object? Value { get; set; }

    /// <summary>How this field should be rendered</summary>
    public FieldType Type { get; set; }

    /// <summary>Visibility/importance level</summary>
    public FieldImportance Importance { get; set; }

    /// <summary>Optional rendering hints (e.g., { "colorThreshold": 400, "color": "red" })</summary>
    public Dictionary<string, object>? Hints { get; set; }
}
