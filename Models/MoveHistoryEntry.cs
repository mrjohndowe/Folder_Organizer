namespace FolderOrganizer.Models;

public class MoveHistoryEntry
{
    public long Id { get; set; }

    public long RunId { get; set; }

    public string SourcePath { get; set; } = string.Empty;

    public string? DestinationPath { get; set; }

    public string? FinalDestinationPath { get; set; }

    public string FileType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}