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
            throw new InvalidOperationException(
                "Automatic folder moves are not allowed.");
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
}