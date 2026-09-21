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
                MessageBox.Show(
                    $"Could not parse remote version: {tagName}",
                    "Update Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
                    "but no installer was found in the GitHub release.\n\n" +
                    "Please download manually from:\n" +
                    $"https://github.com/{GitHubOwner}/{GitHubRepository}/releases/latest",
                    "Update Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var result = MessageBox.Show(
                $"Folder Organizer {remoteVersion} is available (current: {currentVersion}).\n\n" +
                "Would you like to download and install the update?",
                "Update Available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await DownloadAndInstallAsync(
                    installerUrl,
                    remoteVersion);
            }
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show(
                $"Could not connect to GitHub to check for updates.\n\n" +
                $"Error: {ex.Message}\n\n" +
                "Please check your internet connection or visit:\n" +
                $"https://github.com/{GitHubOwner}/{GitHubRepository}/releases",
                "Update Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Folder Organizer could not check for updates.\n\n" +
                $"Error: {ex.Message}\n\n" +
                "Stack trace:\n" + ex.StackTrace,
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
            ?? new Version(0, 1, 1, 0);
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

        try
        {
            // Download the installer
            using (var response =
                await HttpClient.GetAsync(
                    installerUrl,
                    HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                await using (Stream input =
                    await response.Content.ReadAsStreamAsync())
                {
                    await using (FileStream output =
                        new FileStream(
                            installerPath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, bytesRead);
                        }

                        await output.FlushAsync();
                    }
                }
            }

            // Verify the installer was downloaded
            if (!File.Exists(installerPath))
            {
                throw new InvalidOperationException(
                    "Installer was not downloaded successfully.");
            }

            var fileInfo = new FileInfo(installerPath);
            if (fileInfo.Length < 1024) // Less than 1KB is suspicious
            {
                throw new InvalidOperationException(
                    $"Downloaded installer is too small ({fileInfo.Length} bytes). " +
                    "It may not be a valid installer.");
            }
        }
        catch (Exception ex)
        {
            // Clean up failed download
            try
            {
                if (File.Exists(installerPath))
                {
                    File.Delete(installerPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }

            throw new InvalidOperationException(
                $"Failed to download installer: {ex.Message}", ex);
        }

        var startInfo =
        new ProcessStartInfo
        {
            FileName = installerPath,
            UseShellExecute = true,

            Arguments =
                "/VERYSILENT " +
                "/SUPPRESSMSGBOXES " +
                "/NORESTART " +
                "/CLOSEAPPLICATIONS"
            };

        Process? process = Process.Start(startInfo);

        if (process == null)
        {
            throw new InvalidOperationException(
                "The installer could not be started.");
        }

        Application.Current.Shutdown();
    }

    public static async Task CheckForUpdatesManualAsync()
    {
        await CheckForUpdatesAsync(showUpToDateMessage: true);
    }
}