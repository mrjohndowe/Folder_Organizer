namespace FolderOrganizer.Models;

public class OrganizationRun
{
    public long Id { get; set; }

    public string RootFolder { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int TotalItems { get; set; }

    public int PlannedMoves { get; set; }

    public int SuccessfulMoves { get; set; }

    public int FailedMoves { get; set; }
}