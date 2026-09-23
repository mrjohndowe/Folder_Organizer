using System.Collections.ObjectModel;

namespace FolderOrganizer.Models;

/// <summary>
/// A collapsible presentation node for one scanned file or folder.
/// The underlying operation remains the single source of truth for applying moves.
/// </summary>
public class ScanTreeNode
{
    public ScanTreeNode(MoveOperation operation)
    {
        Operation = operation;
        Name = operation.Name;
    }

    private ScanTreeNode(string rootPath)
    {
        Name = rootPath;
        IsRoot = true;
        IsExpanded = true;
    }

    public MoveOperation? Operation { get; }

    public string Name { get; }

    public bool IsRoot { get; }

    public bool HasOperation => Operation is not null;

    public bool IsDirectory => Operation?.IsDirectory == true;

    public string IconPath => Operation?.IconPath ?? string.Empty;

    public ObservableCollection<ScanTreeNode> Children { get; } = [];

    /// <summary>
    /// Lets people mark more than one branch for the actions at the bottom of the window.
    /// This replaces the multiple-row selection provided by the old table.
    /// </summary>
    public bool IsMarked { get; set; }

    public bool IsExpanded { get; set; } = true;

    public bool Selected
    {
        get => Operation?.Selected == true;
        set
        {
            if (Operation is not null)
            {
                Operation.Selected = value;
            }
        }
    }

    public bool CanApply =>
        Operation?.Action.Equals("MOVE", StringComparison.OrdinalIgnoreCase) == true;

    public string Action => IsRoot ? "SCANNED FOLDER" : Operation?.Action ?? string.Empty;

    public string Reason => IsRoot
        ? "Collapse this folder to hide all scan results."
        : Operation?.Reason ?? string.Empty;

    public string Summary =>
        IsRoot
            ? "Expand to view the files and folders found in this scan."
            : CanApply && !string.IsNullOrWhiteSpace(Operation?.DestinationPath)
                ? $"{Operation.Action}  →  {Operation.DestinationPath}"
                : Operation?.Action ?? string.Empty;

    public static ScanTreeNode CreateRoot(string path) => new(path);
}
