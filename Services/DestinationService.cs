using System.IO;

namespace FolderOrganizer.Services;

public class DestinationService
{
    public string GetAvailableDestination(string desiredPath)
    {
        if (!File.Exists(desiredPath) &&
            !Directory.Exists(desiredPath))
        {
            return desiredPath;
        }

        var directory =
            Path.GetDirectoryName(desiredPath)
            ?? throw new InvalidOperationException(
                "Destination directory could not be determined.");

        var fileName =
            Path.GetFileNameWithoutExtension(desiredPath);

        var extension =
            Path.GetExtension(desiredPath);

        var number = 1;

        while (true)
        {
            var candidateName =
                $"{fileName} ({number}){extension}";

            var candidatePath =
                Path.Combine(directory, candidateName);

            if (!File.Exists(candidatePath) &&
                !Directory.Exists(candidatePath))
            {
                return candidatePath;
            }

            number++;
        }
    }
}