using System.IO;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class ScanService
{
    private readonly ClassificationService _classificationService;

    public ScanService(ClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public Task<List<MoveOperation>> ScanAsync(
        string rootFolder,
        bool includeSubfolders)
    {
        return Task.Run(() =>
        {
            var results = new List<MoveOperation>();

            if (!Directory.Exists(rootFolder))
            {
                return results;
            }

            var searchOption = includeSubfolders
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            try
            {
                // Folders
                foreach (var directory in
                         Directory.EnumerateDirectories(
                             rootFolder,
                             "*",
                             searchOption))
                {
                    try
                    {
                        var info = new DirectoryInfo(directory);

                        results.Add(
                            _classificationService.Classify(
                                info,
                                rootFolder));
                    }
                    catch
                    {
                        // An inaccessible folder should not kill the scan.
                    }
                }

                // Files
                foreach (var file in
                         Directory.EnumerateFiles(
                             rootFolder,
                             "*",
                             searchOption))
                {
                    try
                    {
                        var info = new FileInfo(file);

                        results.Add(
                            _classificationService.Classify(
                                info,
                                rootFolder));
                    }
                    catch
                    {
                        // Same philosophy:
                        // skip one bad file instead of detonating the app.
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // We'll add proper logging/status messages next.
            }

            return results
                .OrderBy(x => x.IsDirectory ? 0 : 1)
                .ThenBy(x => x.Name)
                .ToList();
        });
    }
}
