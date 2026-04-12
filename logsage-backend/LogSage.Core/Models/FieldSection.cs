namespace LogSage.Core.Models;

/// <summary>
/// Groups related fields into a logical section for display
/// </summary>
public class FieldSection
{
    /// <summary>Section title (e.g., "Request Context", "Exception Details")</summary>
    public string SectionName { get; set; } = string.Empty;

    /// <summary>Display order (0 = first, higher numbers = later)</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Fields in this section</summary>
    public List<DisplayField> Fields { get; set; } = [];
}
