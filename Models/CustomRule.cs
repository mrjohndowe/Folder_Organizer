namespace FolderOrganizer.Models;

public class CustomRule
{
    public long Id { get; set; }

    public string FolderName { get; set; } = string.Empty;

    public string Extensions { get; set; } = string.Empty;

    public int Priority { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public RuleAction Action { get; set; } = RuleAction.Move;

    public bool IsRemoval
    {
        get => Action == RuleAction.Remove;

        set =>
            Action = value
                ? RuleAction.Remove
                : RuleAction.Move;
    }
}