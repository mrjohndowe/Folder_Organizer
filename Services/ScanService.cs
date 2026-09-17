using System.IO;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class ScanService
{
    private readonly ClassificationService _classificationService;

    private static readonly HashSet<string> ProtectedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase)
         {
            ".git",
            ".github",
            ".svn",
            ".hg",
            ".vs",
            ".idea",
            ".vscode",
            "bin",
            "obj",
            "node_modules",
            "$Recycle.Bin",
            "System Volume Information"
        };

    public ScanService(
        ClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public Task<List<MoveOperation>> ScanAsync(
        string rootFolder,
        bool includeSubfolders,
        HashSet<string>? ignoredPaths = null)
    {
        return Task.Run(() =>
        {
            ignoredPaths ??=
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var results = new List<MoveOperation>();

            if (!Directory.Exists(rootFolder))
            {
                return results;
            }

            var rootDirectory =
                new DirectoryInfo(rootFolder);

            ScanDirectory(
                rootDirectory,
                rootFolder,
                includeSubfolders,
                results,
                ignoredPaths);

            return results
                .OrderBy(x => x.IsDirectory ? 0 : 1)
                .ThenBy(x => x.Name)
                .ToList();
        });
    }


    private void ScanDirectory(
        DirectoryInfo directory,
        string rootFolder,
        bool includeSubfolders,
        List<MoveOperation> results,
        HashSet<string> ignoredPaths)
    {
        FileSystemInfo[] entries;

        try
        {
            entries = directory.GetFileSystemInfos();
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }

        foreach (var entry in entries)
        {
            try
            {
                if (entry is DirectoryInfo subdirectory)
                {
                    var operation =
                        _classificationService.Classify(
                            subdirectory,
                            rootFolder);

                    if (ProtectedDirectoryNames.Contains(
                            subdirectory.Name))
                    {
                        operation.Action = "IGNORE";
                        operation.Selected = false;
                        operation.Reason =
                            "Protected development/system folder. " +
                            "Contents were not scanned.";

                        results.Add(operation);
                        continue;
                    }

                    if ((subdirectory.Attributes &
                         FileAttributes.ReparsePoint) != 0)
                    {
                        operation.Action = "IGNORE";
                        operation.Selected = false;
                        operation.Reason =
                            "Linked/junction folder. " +
                            "Contents were not scanned.";

                        results.Add(operation);
                        continue;
                    }

                    results.Add(operation);

                    if (includeSubfolders)
                    {
                        ScanDirectory(
                            subdirectory,
                            rootFolder,
                            true,
                            results,
                            ignoredPaths);
                    }
                }
                else if (entry is FileInfo file)
                {
                    results.Add(
                        _classificationService.Classify(
                            file,
                            rootFolder));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip inaccessible items.
            }
            catch (IOException)
            {
                // Skip files/folders that disappear or become locked.
            }
        }
    }
}