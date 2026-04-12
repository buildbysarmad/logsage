namespace LogSage.Core.Models;

/// <summary>
/// Defines how a field should be rendered in the UI
/// </summary>
public enum FieldType
{
    /// <summary>Plain text value</summary>
    Text,

    /// <summary>Numeric value</summary>
    Number,

    /// <summary>Time duration (e.g., "00:05:23")</summary>
    Duration,

    /// <summary>ISO timestamp</summary>
    Timestamp,

    /// <summary>URL or path</summary>
    Url,

    /// <summary>Multi-line stack trace</summary>
    StackTrace,

    /// <summary>JSON payload</summary>
    Json,

    /// <summary>Exception type name</summary>
    ExceptionType
}
