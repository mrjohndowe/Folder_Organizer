using System.IO;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class ClassificationService
{
    private SpecialRule? _specialRule;

    public void SetSpecialRule(SpecialRule specialRule)
    {
        _specialRule = specialRule;
    }
    private static readonly HashSet<string> DocumentExtensions =
    [
        ".pdf",
        ".doc",
        ".docx",
        ".txt",
        ".rtf",
        ".odt",
        ".xls",
        ".xlsx",
        ".csv",
        ".ppt",
        ".pptx"
    ];

    private static readonly HashSet<string> ImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".webp",
        ".tif",
        ".tiff",
        ".svg"
    ];

    private static readonly HashSet<string> VideoExtensions =
    [
        ".mp4",
        ".mkv",
        ".avi",
        ".mov",
        ".wmv",
        ".webm",
        ".m4v"
    ];

    private static readonly HashSet<string> AudioExtensions =
    [
        ".mp3",
        ".wav",
        ".flac",
        ".aac",
        ".ogg",
        ".m4a",
        ".wma"
    ];

    private static readonly HashSet<string> ArchiveExtensions =
    [
        ".zip",
        ".rar",
        ".7z",
        ".tar",
        ".gz"
    ];

    public MoveOperation Classify(
        FileSystemInfo item,
        string rootFolder,
        IReadOnlyList<CustomRule>? customRules = null)
    {
        if (item is DirectoryInfo)
        {
            return new MoveOperation
            {
                Selected = false,
                Name = item.Name,
                SourcePath = item.FullName,
                Type = "Folder",
                Action = "IGNORE",
                DestinationPath = string.Empty,
                Reason = "Folders are not automatically moved.",
                IsDirectory = true
            };
        }

        var extension = Path.GetExtension(item.Name).ToLowerInvariant();
        customRules ??= [];

        // Check special rule first (date-based organization)
        var specialRuleResult = ApplySpecialRule(
            item,
            extension,
            rootFolder);

        if (specialRuleResult != null)
        {
            return specialRuleResult;
        }

        var matchingCustomRule =
            customRules
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Priority)
                .FirstOrDefault(x =>
                    RuleMatchesExtension(
                        x.Extensions,
                        extension));

        if (matchingCustomRule != null)
        {
            var customDestinationDirectory =
                Path.Combine(
                    rootFolder,
                    matchingCustomRule.FolderName);

            var customDestination =
                Path.Combine(
                    customDestinationDirectory,
                    item.Name);

            if (Path.GetDirectoryName(item.FullName)?
                    .Equals(
                        customDestinationDirectory,
                        StringComparison.OrdinalIgnoreCase) == true)
            {
                return new MoveOperation
                {
                    Selected = false,
                    Name = item.Name,
                    SourcePath = item.FullName,
                    Type = extension.TrimStart('.').ToUpperInvariant(),
                    Action = "IGNORE",
                    DestinationPath = item.FullName,
                    Reason =
                        $"File is already in custom folder " +
                        $"{matchingCustomRule.FolderName}.",
                    IsDirectory = false
                };
            }

            return new MoveOperation
            {
                Selected = true,
                Name = item.Name,
                SourcePath = item.FullName,
                Type = extension.TrimStart('.').ToUpperInvariant(),
                Action = "MOVE",
                DestinationPath = customDestination,
                Reason =
                    $"Custom rule #{matchingCustomRule.Priority}: " +
                    $"move to {matchingCustomRule.FolderName}.",
                IsDirectory = false
            };
        }

        string? destinationFolder = null;
        string type;

        if (DocumentExtensions.Contains(extension))
        {
            destinationFolder = "Documents";

            type = extension == ".pdf"
                ? "PDF"
                : "Document";
        }
        else if (ImageExtensions.Contains(extension))
        {
            destinationFolder = "Pictures";
            type = "Image";
        }
        else if (VideoExtensions.Contains(extension))
        {
            destinationFolder = "Videos";
            type = "Video";
        }
        else if (AudioExtensions.Contains(extension))
        {
            destinationFolder = "Music";
            type = "Audio";
        }
        else if (ArchiveExtensions.Contains(extension))
        {
            destinationFolder = "Archives";
            type = "Archive";
        }
        else
        {
            return new MoveOperation
            {
                Selected = false,
                Name = item.Name,
                SourcePath = item.FullName,
                Type = string.IsNullOrWhiteSpace(extension)
                    ? "File"
                    : extension.TrimStart('.').ToUpperInvariant(),
                Action = "REVIEW",
                DestinationPath = string.Empty,
                Reason = "No organization rule exists for this file type.",
                IsDirectory = false
            };
        }

        var destinationDirectory =
            Path.Combine(rootFolder, destinationFolder);

        var destination =
            Path.Combine(destinationDirectory, item.Name);

        // Already organized? Leave it alone.
        if (Path.GetDirectoryName(item.FullName)?
                .Equals(
                    destinationDirectory,
                    StringComparison.OrdinalIgnoreCase) == true)
        {
            return new MoveOperation
            {
                Selected = false,
                Name = item.Name,
                SourcePath = item.FullName,
                Type = type,
                Action = "IGNORE",
                DestinationPath = item.FullName,
                Reason = "File is already in the correct folder.",
                IsDirectory = false
            };
        }

        return new MoveOperation
        {
            Selected = true,
            Name = item.Name,
            SourcePath = item.FullName,
            Type = type,
            Action = "MOVE",
            DestinationPath = destination,
            Reason = $"Move {type} to {destinationFolder}.",
            IsDirectory = false
        };
    }

    private static bool RuleMatchesExtension(
    string extensions,
    string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(extensions) ||
            string.IsNullOrWhiteSpace(fileExtension))
        {
            return false;
        }

        var ruleExtensions =
            extensions.Split(
                [',', ';', ' '],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        foreach (var ruleExtension in ruleExtensions)
        {
            var normalized =
                ruleExtension.StartsWith('.')
                    ? ruleExtension.ToLowerInvariant()
                    : "." + ruleExtension.ToLowerInvariant();

            if (normalized.Equals(
                    fileExtension,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private MoveOperation? ApplySpecialRule(
        FileSystemInfo item,
        string extension,
        string rootFolder)
    {
        if (_specialRule == null || !_specialRule.IsEnabled)
        {
            return null;
        }

        // If year organization is not enabled, don't apply the rule
        if (!_specialRule.OrganizeByYear)
        {
            return null;
        }

        // Check if file extension matches special rule extensions
        if (!string.IsNullOrWhiteSpace(_specialRule.Extensions))
        {
            var ruleExtensions =
                _specialRule.Extensions.Split(
                    [',', ';', ' '],
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            var matchesExtension = false;
            foreach (var ruleExtension in ruleExtensions)
            {
                var normalized =
                    ruleExtension.StartsWith('.')
                        ? ruleExtension.ToLowerInvariant()
                        : "." + ruleExtension.ToLowerInvariant();

                if (normalized.Equals(
                        extension,
                        StringComparison.OrdinalIgnoreCase))
                {
                    matchesExtension = true;
                    break;
                }
            }

            if (!matchesExtension)
            {
                return null; // Extension doesn't match, don't apply special rule
            }
        }

        // Get the appropriate date
        DateTime fileDate;
        if (item is FileInfo fileInfo)
        {
            fileDate = _specialRule.UseCreationDate
                ? fileInfo.CreationTime
                : fileInfo.LastWriteTime;
        }
        else
        {
            return null; // Only apply to files
        }

        // Generate folder structure based on settings
        var year = fileDate.Year.ToString();
        var month = fileDate.Month.ToString("00"); // Zero-padded month (01-12)
        var monthName = fileDate.ToString("MMMM"); // Full month name

        string destinationFolder;

        if (_specialRule.OrganizeByMonth)
        {
            // Year/Month structure
            destinationFolder = Path.Combine(rootFolder, year, monthName);
        }
        else if (_specialRule.OrganizeByYear)
        {
            // Year only structure
            destinationFolder = Path.Combine(rootFolder, year);
        }
        else
        {
            return null; // No date organization enabled
        }

        var destination = Path.Combine(destinationFolder, item.Name);

        // Check if file is already in the correct location
        if (Path.GetDirectoryName(item.FullName)?
                .Equals(
                    destinationFolder,
                    StringComparison.OrdinalIgnoreCase) == true)
        {
            return new MoveOperation
            {
                Selected = false,
                Name = item.Name,
                SourcePath = item.FullName,
                Type = extension.TrimStart('.').ToUpperInvariant(),
                Action = "IGNORE",
                DestinationPath = item.FullName,
                Reason = "File is already in the correct date-based folder.",
                IsDirectory = false
            };
        }

        return new MoveOperation
        {
            Selected = true,
            Name = item.Name,
            SourcePath = item.FullName,
            Type = extension.TrimStart('.').ToUpperInvariant(),
            Action = "MOVE",
            DestinationPath = destination,
            Reason = $"Special rule: organize by {(_specialRule.OrganizeByMonth ? "year and month" : "year")} ({year}{(_specialRule.OrganizeByMonth ? $" - {monthName}" : "")}).",
            IsDirectory = false
        };
    }

}
