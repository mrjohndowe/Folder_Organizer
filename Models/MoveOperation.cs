namespace FolderOrganizer.Models;

public class MoveOperation
{
    public bool Selected { get; set; } = true;

    public string Name { get; set; } = string.Empty;

    public string SourcePath { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string DestinationPath { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public bool IsDirectory { get; set; }

    public bool IsIgnored =>
        Action.Equals("IGNORE", StringComparison.OrdinalIgnoreCase);
}
