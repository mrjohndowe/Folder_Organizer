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
        "FolderOrganizerSetup.exe";

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
                    $"Version {remoteVersion} is available, " +
                    "but the installer could not be found " +
                    "in the GitHub release.",
                    "Update Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    $"Folder Organizer {remoteVersion} " +
                    "is available.\n\n" +
                    $"Installed version: {currentVersion}\n" +
                    $"New version: {remoteVersion}\n\n" +
                    "Download and install the update now?",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information,
                    MessageBoxResult.Yes);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            await DownloadAndInstallAsync(
                installerUrl,
                remoteVersion);
        }
        catch (Exception ex)
        {
            if (showUpToDateMessage)
            {
                MessageBox.Show(
                    $"Folder Organizer could not check " +
                    $"for updates.\n\n{ex.Message}",
                    "Update Check Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
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

        foreach (JsonElement asset
                 in assets.EnumerateArray())
        {
            string name =
                asset.GetProperty("name")
                    .GetString()
                ?? string.Empty;

            if (!string.Equals(
                    name,
                    InstallerAssetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return asset
                .GetProperty(
                    "browser_download_url")
                .GetString();
        }

        return null;
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