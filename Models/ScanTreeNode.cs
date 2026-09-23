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
    }

    public MoveOperation Operation { get; }

    public ObservableCollection<ScanTreeNode> Children { get; } = [];

    /// <summary>
    /// Lets people mark more than one branch for the actions at the bottom of the window.
    /// This replaces the multiple-row selection provided by the old table.
    /// </summary>
    public bool IsMarked { get; set; }

    public bool IsExpanded { get; set; } = true;

    public bool CanApply =>
        Operation.Action.Equals("MOVE", StringComparison.OrdinalIgnoreCase);

    public string Summary =>
        CanApply && !string.IsNullOrWhiteSpace(Operation.DestinationPath)
            ? $"{Operation.Action}  →  {Operation.DestinationPath}"
            : Operation.Action;
}
