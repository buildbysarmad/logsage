namespace LogSage.Core.Models;

/// <summary>
/// Defines the importance/visibility level of a field in the UI
/// </summary>
public enum FieldImportance
{
    /// <summary>Always show prominently (e.g., timestamp, level, message)</summary>
    Primary,

    /// <summary>Show in detail view (e.g., requestId, source)</summary>
    Secondary,

    /// <summary>Only show in expanded/debug mode</summary>
    Debug
}
