namespace FolderOrganizer.Models;

public class SpecialRule
{
    public long Id { get; set; }

    /// <summary>
    /// Whether the year-based organization is enabled
    /// </summary>
    public bool OrganizeByYear { get; set; }

    /// <summary>
    /// Whether the month-based organization is enabled (requires OrganizeByYear)
    /// </summary>
    public bool OrganizeByMonth { get; set; }

    /// <summary>
    /// Whether to use creation date or modification date
    /// </summary>
    public bool UseCreationDate { get; set; } = true;

    /// <summary>
    /// File extensions to apply this rule to (empty = all files)
    /// </summary>
    public string Extensions { get; set; } = string.Empty;

    /// <summary>
    /// Whether this rule is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// When this rule was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this rule was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}