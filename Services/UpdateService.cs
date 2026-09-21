using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace FolderOrganizer.Services;

public static class UpdateService
{
    private const string GitHubOwner =
        "mrjohndowe";

    private const string GitHubRepository =
        "Folder_Organizer";

    private const string InstallerAssetName =
        "FolderOrganizer-Setup.exe";

    private static readonly HttpClient HttpClient =
        new();

    public static async Task CheckForUpdatesAsync(
    bool showUpToDateMessage = false)
    {
        try
        {
            ConfigureHttpClient();

            string url =
                $"https://api.github.com/repos/" +
                $"{GitHubOwner}/" +
                $"{GitHubRepository}/" +
                $"releases/latest";

            using var response =
                await HttpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json =
                await response.Content.ReadAsStringAsync();

            using JsonDocument document =
                JsonDocument.Parse(json);

            JsonElement root =
                document.RootElement;

            string tagName =
                root.GetProperty("tag_name")
                    .GetString()
                ?? string.Empty;

            string remoteVersionText =
                tagName.TrimStart(
                    'v',
                    'V');

            if (!Version.TryParse(
                    remoteVersionText,
                    out Version? remoteVersion))
            {
                return;
            }

            Version currentVersion =
                GetCurrentVersion();

            if (remoteVersion <= currentVersion)
            {
                if (showUpToDateMessage)
                {
                    MessageBox.Show(
                        $"Folder Organizer {currentVersion} " +
                        "is already the latest version.",
                        "Folder Organizer",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return;
            }

            string? installerUrl =
                FindInstallerUrl(root);

            if (string.IsNullOrWhiteSpace(
                    installerUrl))
            {
                MessageBox.Show(
                    $"Folder Organizer {remoteVersion} is available, " +
                    "but no installer was found in the GitHub release.",
                    "Update Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            await DownloadAndInstallAsync(
                installerUrl,
                remoteVersion);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Folder Organizer could not update automatically.\n\n" +
                $"{ex.Message}",
                "Update Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private static void ConfigureHttpClient()
    {
        if (HttpClient.DefaultRequestHeaders
            .UserAgent.Count > 0)
        {
            return;
        }

        HttpClient.DefaultRequestHeaders
            .UserAgent
            .Add(
                new ProductInfoHeaderValue(
                    "FolderOrganizer",
                    GetCurrentVersion()
                        .ToString()));
    }

    private static Version GetCurrentVersion()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version
            ?? new Version(0, 0, 0, 1);
    }

    private static string? FindInstallerUrl(
    JsonElement root)
    {
        if (!root.TryGetProperty(
                "assets",
                out JsonElement assets))
        {
            return null;
        }

        string? fallbackInstallerUrl =
            null;

        foreach (JsonElement asset
                 in assets.EnumerateArray())
        {
            string name =
                asset.GetProperty("name")
                    .GetString()
                ?? string.Empty;

            string? downloadUrl =
                asset.GetProperty(
                        "browser_download_url")
                    .GetString();

            if (string.Equals(
                    name,
                    InstallerAssetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return downloadUrl;
            }

            if (name.EndsWith(
                    ".exe",
                    StringComparison.OrdinalIgnoreCase))
            {
                fallbackInstallerUrl ??=
                    downloadUrl;
            }
        }

        return fallbackInstallerUrl;
    }

    private static async Task DownloadAndInstallAsync(
        string installerUrl,
        Version version)
    {
        string updateFolder =
            Path.Combine(
                Path.GetTempPath(),
                "FolderOrganizer",
                "Updates",
                version.ToString());

        Directory.CreateDirectory(
            updateFolder);

        string installerPath =
            Path.Combine(
                updateFolder,
                InstallerAssetName);

        using var response =
            await HttpClient.GetAsync(
                installerUrl,
                HttpCompletionOption
                    .ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        await using Stream input =
            await response.Content
                .ReadAsStreamAsync();

        await using FileStream output =
            File.Create(installerPath);

        await input.CopyToAsync(output);

        await output.FlushAsync();

        var startInfo =
            new ProcessStartInfo
            {
                FileName =
                    installerPath,

                UseShellExecute =
                    true,

                Arguments =
                    "/VERYSILENT " +
                    "/SUPPRESSMSGBOXES " +
                    "/NORESTART " +
                    "/CLOSEAPPLICATIONS"
            };

        Process.Start(startInfo);

        Application.Current.Shutdown();
    }
}