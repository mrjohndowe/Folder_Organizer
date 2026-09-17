using System.IO;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class ClassificationService
{
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
        string rootFolder)
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
}
