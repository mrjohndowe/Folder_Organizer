using System.IO;
using System.Windows;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class FileIconService
{
    public readonly FileIconService _fileIconService;
    

        public FileIconService(
            FileIconService fileIconService)
        {
            _fileIconService = fileIconService;

        }    


    

    private static readonly Dictionary<string, string> IconAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["mdb"] = "access",
            ["accdb"] = "access",

            ["as"] = "actionscript",

            ["ai"] = "ai",

            ["afdesign"] = "affinitydesigner",
            ["afphoto"] = "affinityphoto",
            ["afpub"] = "affinitypublisher",

            ["scpt"] = "applescript",
            ["applescript"] = "applescript",

            ["ino"] = "arduino",

            ["asp"] = "asp",
            ["aspx"] = "aspx",

            ["asm"] = "assembly",

            ["ahk"] = "ahk"
        };

    public static string GetIconPath(string? filePath)
    {
        const string defaultIcon =
            "pack://application:,,,/Assets/file.svg";

        const string folderIcon =
            "pack://application:,,,/Assets/folder.svg";

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return defaultIcon;
        }

        if (Directory.Exists(filePath))
        {
            return folderIcon;
        }

        if (Directory.Exists(filePath) ||
         filePath.EndsWith(Path.DirectorySeparatorChar) ||
         filePath.EndsWith(Path.AltDirectorySeparatorChar))
            {
                return folderIcon;
            }

        var extension = Path
            .GetExtension(filePath)
            .TrimStart('.')
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension))
        {
            return defaultIcon;
        }

        var iconName =
            IconAliases.TryGetValue(extension, out var alias)
                ? alias
                : extension;

        var iconPath =
            $"pack://application:,,,/Assets/{iconName}.svg";

        try
        {
            var uri = new Uri(
                iconPath,
                UriKind.Absolute);

            return Application.GetResourceStream(uri) != null
                ? iconPath
                : defaultIcon;
        }
        catch
        {
            return defaultIcon;
        }
    }


}






