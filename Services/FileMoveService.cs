using System.IO;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class FileMoveService
{
    private readonly DestinationService _destinationService;

    public FileMoveService(
        DestinationService destinationService)
    {
        _destinationService = destinationService;
    }

    public string Move(MoveOperation operation)
    {
        if (!operation.Selected)
        {
            throw new InvalidOperationException(
                "The item is not selected.");
        }

        if (!operation.Action.Equals(
                "MOVE",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only MOVE operations may be applied.");
        }

        if (operation.IsDirectory)
        {
            return MoveDirectory(operation);
        }

        if (!File.Exists(operation.SourcePath))
        {
            throw new FileNotFoundException(
                "The source file no longer exists.",
                operation.SourcePath);
        }

        var destination =
            _destinationService.GetAvailableDestination(
                operation.DestinationPath);

        var destinationDirectory =
            Path.GetDirectoryName(destination);

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            throw new InvalidOperationException(
                "Could not determine the destination directory.");
        }

        Directory.CreateDirectory(destinationDirectory);

        File.Move(
            operation.SourcePath,
            destination);

        return destination;
    }

    private string MoveDirectory(MoveOperation operation)
    {
        if (!Directory.Exists(operation.SourcePath))
        {
            throw new DirectoryNotFoundException(
                "The source folder no longer exists: " +
                operation.SourcePath);
        }

        var source = Path.GetFullPath(operation.SourcePath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var desiredDestination = Path.GetFullPath(operation.DestinationPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (desiredDestination.Equals(
                source,
                StringComparison.OrdinalIgnoreCase) ||
            desiredDestination.StartsWith(
                source + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A folder cannot be moved into itself or one of its subfolders.");
        }

        var destination =
            _destinationService.GetAvailableDestination(
                desiredDestination);

        var destinationParent = Path.GetDirectoryName(destination);

        if (string.IsNullOrWhiteSpace(destinationParent))
        {
            throw new InvalidOperationException(
                "Could not determine the destination folder.");
        }

        Directory.CreateDirectory(destinationParent);
        Directory.Move(source, destination);

        return destination;
    }
}
