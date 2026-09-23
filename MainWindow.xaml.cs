using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FolderOrganizer.Models;
using FolderOrganizer.Services;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Reflection;
using System.Diagnostics;

namespace FolderOrganizer;

public partial class MainWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly ObservableCollection<MoveOperation> _operations = [];
    public ObservableCollection<ScanTreeNode> ScanTree { get; } = [];
    private readonly DestinationService _destinationService;
    private readonly FileMoveService _fileMoveService;

    private readonly ClassificationService _classificationService;
    private readonly ScanService _scanService;
    private SpecialRule? _currentSpecialRule;

    public MainWindow()
    {
        InitializeComponent();

        _classificationService = new ClassificationService();
        _scanService = new ScanService(_classificationService);
        _databaseService = new DatabaseService();
        _destinationService = new DestinationService();
        _fileMoveService = new FileMoveService(_destinationService);

        Loaded += MainWindow_Loaded;

        DataContext = this;
    }

    private void BrowseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Folder to Organize",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            FolderPathTextBox.Text = dialog.FolderName;
        }
    }

    private void SettingsButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var settingsWindow =
            new SettingsWindow(_databaseService)
            {
                Owner = this
            };

        settingsWindow.ShowDialog();
    }

    private async void CheckUpdatesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await UpdateService.CheckForUpdatesManualAsync();
    }

    private async void RemoveIgnoreButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var selectedItems = GetMarkedOperations();

        if (selectedItems.Count == 0)
        {
            MessageBox.Show(
                "Select one or more ignored files or folders first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        foreach (var item in selectedItems)
        {
            await _databaseService.RemoveIgnoredPathAsync(
                item.SourcePath);
        }

        await RefreshScanAsync();
    }

    private async void IgnoreSelectedButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var selectedItems = GetMarkedOperations();

        if (selectedItems.Count == 0)
        {
            MessageBox.Show(
                "Select one or more files or folders first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        foreach (var item in selectedItems)
        {
            await _databaseService.AddIgnoredPathAsync(
                item.SourcePath);
        }

        await RefreshScanAsync();
    }

    private void MarkFoldersForRemovalButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var folders = GetSelectedFolders();

        if (folders.Count == 0)
        {
            MessageBox.Show(
                "Select one or more folders first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        foreach (var folder in folders)
        {
            folder.Action = "REMOVE";
            folder.Selected = false;
            folder.DestinationPath = string.Empty;
            folder.Reason =
                "Marked for removal. This is only a review marker; " +
                "Folder Organizer will not delete it.";
        }

        RebuildScanTree();
        StatusTextBlock.Text =
            $"{folders.Count:N0} folder(s) marked for removal review. " +
            "Nothing was deleted.";
    }

    private void MoveFoldersButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var folders = GetSelectedFolders();

        if (folders.Count == 0)
        {
            MessageBox.Show(
                "Select one or more folders first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var dialog = new OpenFolderDialog
        {
            Title = "Select Where to Move the Folder"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var destinationRoot = dialog.FolderName;

        foreach (var folder in folders)
        {
            var destination = Path.Combine(destinationRoot, folder.Name);

            if (IsSameOrChildPath(destination, folder.SourcePath))
            {
                MessageBox.Show(
                    $"{folder.Name} cannot be moved into itself or one " +
                    "of its subfolders.",
                    "Folder Organizer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                continue;
            }

            folder.Action = "MOVE";
            folder.Selected = true;
            folder.DestinationPath = destination;
            folder.Reason =
                "Move this entire folder and all of its contents.";

            MarkDescendantsCoveredByFolderMove(folder);
        }

        RebuildScanTree();
        UpdateApplyButton();
    }

    private List<MoveOperation> GetSelectedFolders() =>
        GetMarkedOperations()
            .Where(x => x.IsDirectory)
            .ToList();

    private List<MoveOperation> GetMarkedOperations() =>
        FlattenTreeNodes(ScanTree)
            .Where(x => x.Selected && x.Operation is not null)
            .Select(x => x.Operation!)
            .ToList();

    private static IEnumerable<ScanTreeNode> FlattenTreeNodes(
        IEnumerable<ScanTreeNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;

            foreach (var child in FlattenTreeNodes(node.Children))
            {
                yield return child;
            }
        }
    }

    private void RebuildScanTree()
    {
        ScanTree.Clear();

        var nodesByPath = _operations.ToDictionary(
            x => Path.GetFullPath(x.SourcePath),
            x => new ScanTreeNode(x),
            StringComparer.OrdinalIgnoreCase);

        var rootNodes = new List<ScanTreeNode>();

        foreach (var operation in _operations)
        {
            var node = nodesByPath[Path.GetFullPath(operation.SourcePath)];
            var parentPath = Path.GetDirectoryName(operation.SourcePath);

            if (!string.IsNullOrWhiteSpace(parentPath) &&
                nodesByPath.TryGetValue(Path.GetFullPath(parentPath), out var parent))
            {
                parent.Children.Add(node);
            }
            else
            {
                rootNodes.Add(node);
            }
        }

        var scanRoot = ScanTreeNode.CreateRoot(
            FolderPathTextBox.Text.Trim());

        foreach (var rootNode in rootNodes)
        {
            scanRoot.Children.Add(rootNode);
        }

        SortTreeNodes(scanRoot.Children);
        ScanTree.Add(scanRoot);
    }

    private static void SortTreeNodes(
        ObservableCollection<ScanTreeNode> nodes)
    {
        var ordered = nodes
            .OrderBy(x => x.IsDirectory ? 0 : 1)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        nodes.Clear();

        foreach (var node in ordered)
        {
            SortTreeNodes(node.Children);
            nodes.Add(node);
        }
    }

    private void PreviewUseCheckBox_Click(
        object sender,
        RoutedEventArgs e) =>
        UpdateApplyButton();

    private void MarkDescendantsCoveredByFolderMove(
        MoveOperation folder)
    {
        foreach (var child in _operations.Where(x =>
                     !ReferenceEquals(x, folder) &&
                     IsChildPath(x.SourcePath, folder.SourcePath)))
        {
            child.Selected = false;
            child.Action = "IGNORE";
            child.Reason =
                $"Included when folder '{folder.Name}' is moved.";
        }
    }

    private static bool IsSameOrChildPath(
        string candidatePath,
        string parentPath)
    {
        var candidate = Path.GetFullPath(candidatePath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parent = Path.GetFullPath(parentPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return candidate.Equals(parent, StringComparison.OrdinalIgnoreCase) ||
               candidate.StartsWith(
                   parent + Path.DirectorySeparatorChar,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsChildPath(
        string candidatePath,
        string parentPath) =>
        !Path.GetFullPath(candidatePath).Equals(
            Path.GetFullPath(parentPath),
            StringComparison.OrdinalIgnoreCase) &&
        IsSameOrChildPath(candidatePath, parentPath);

    private async void MainWindow_Loaded(
    object sender,
    RoutedEventArgs e)
    {
        try
        {
            await _databaseService.InitializeAsync();
            var applicationVersion =
                await _databaseService.GetApplicationVersionAsync();

            if (!string.IsNullOrWhiteSpace(applicationVersion))
            {
                TitleBarVersionText.Text = $"v{applicationVersion}";
            }

            var darkModeSetting =
            await _databaseService.GetSettingAsync(
                "DarkMode");

            ThemeService.Apply(
                string.Equals(
                    darkModeSetting,
                    "true",
                    StringComparison.OrdinalIgnoreCase));

            // Load special rule settings
            await LoadSpecialRuleSettingsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The history database could not be initialized.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private async Task LoadSpecialRuleSettingsAsync()
    {
        try
        {
            _currentSpecialRule = await _databaseService.GetSpecialRuleAsync();

            // Update UI controls
            OrganizeByYearCheckBox.IsChecked = _currentSpecialRule.OrganizeByYear;
            OrganizeByMonthCheckBox.IsChecked = _currentSpecialRule.OrganizeByMonth;
            OrganizeByMonthCheckBox.IsEnabled = _currentSpecialRule.OrganizeByYear;
            UseCreationDateCheckBox.IsChecked = _currentSpecialRule.UseCreationDate;
            SpecialRuleExtensionsTextBox.Text = _currentSpecialRule.Extensions;

            // Update classification service with current special rule
            _classificationService.SetSpecialRule(_currentSpecialRule);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to load special rule settings.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void OrganizeByYearCheckBox_Checked(
        object sender,
        RoutedEventArgs e)
    {
        OrganizeByMonthCheckBox.IsEnabled = true;
    }

    private void OrganizeByYearCheckBox_Unchecked(
        object sender,
        RoutedEventArgs e)
    {
        OrganizeByMonthCheckBox.IsEnabled = false;
        OrganizeByMonthCheckBox.IsChecked = false;
    }

    private void OrganizeByMonthCheckBox_Checked(
        object sender,
        RoutedEventArgs e)
    {
        // Month organization is now enabled
    }

    private void OrganizeByMonthCheckBox_Unchecked(
        object sender,
        RoutedEventArgs e)
    {
        // Month organization is now disabled
    }

    private void UseCreationDateCheckBox_Checked(
        object sender,
        RoutedEventArgs e)
    {
        // Will use creation date
    }

    private void UseCreationDateCheckBox_Unchecked(
        object sender,
        RoutedEventArgs e)
    {
        // Will use modification date
    }

    private async void ApplySpecialRuleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            if (_currentSpecialRule == null)
            {
                _currentSpecialRule = await _databaseService.GetSpecialRuleAsync();
            }

            _currentSpecialRule.OrganizeByYear = OrganizeByYearCheckBox.IsChecked == true;
            _currentSpecialRule.OrganizeByMonth = OrganizeByMonthCheckBox.IsChecked == true;
            _currentSpecialRule.UseCreationDate = UseCreationDateCheckBox.IsChecked == true;
            _currentSpecialRule.Extensions = SpecialRuleExtensionsTextBox.Text.Trim();

            // Update the special rule in database
            _currentSpecialRule = await _databaseService.UpdateSpecialRuleAsync(_currentSpecialRule);

            // Update classification service
            _classificationService.SetSpecialRule(_currentSpecialRule);

            MessageBox.Show(
                "Special rule settings have been saved and applied.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Refresh scan if a folder is selected
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text) &&
                Directory.Exists(FolderPathTextBox.Text))
            {
                await RefreshScanAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save special rule settings.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task RefreshScanAsync()
    {
        var folder = FolderPathTextBox.Text.Trim();

        if (!Directory.Exists(folder))
        {
            return;
        }

        _operations.Clear();

        StatusTextBlock.Text = "Scanning...";

        // Update classification service with current special rule
        if (_currentSpecialRule != null)
        {
            _classificationService.SetSpecialRule(_currentSpecialRule);
        }

        var ignoredPaths =
            await _databaseService.GetIgnoredPathsAsync();

        var customRules =
            await _databaseService.GetCustomRulesAsync();

        var results =
            await _scanService.ScanAsync(
                folder,
                IncludeSubfoldersCheckBox.IsChecked == true,
                ignoredPaths,
                customRules);

        foreach (var result in results)
        {
            _operations.Add(result);
        }

        RebuildScanTree();

        var moveCount =
            _operations.Count(x =>
                x.Action.Equals(
                    "MOVE",
                    StringComparison.OrdinalIgnoreCase));

        var ignoredCount =
            _operations.Count(x =>
                x.Action.Equals(
                    "IGNORE",
                    StringComparison.OrdinalIgnoreCase));

        var reviewCount =
            _operations.Count(x =>
                x.Action.Equals(
                    "REVIEW",
                    StringComparison.OrdinalIgnoreCase));

        StatusTextBlock.Text =
            $"{_operations.Count:N0} items scanned | " +
            $"{moveCount:N0} moves | " +
            $"{ignoredCount:N0} ignored | " +
            $"{reviewCount:N0} need review";

        UpdateApplyButton();
    }

    private void UpdateApplyButton()
    {
        ApplyButton.IsEnabled =
            _operations.Any(x =>
                x.Selected &&
                x.Action.Equals(
                    "MOVE",
                    StringComparison.OrdinalIgnoreCase));
    }

    private void HistoryButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var historyWindow =
            new HistoryWindow(_databaseService)
            {
                Owner = this
            };

        historyWindow.ShowDialog();
    }

    private async void ApplyButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var operationsToApply =
            _operations
                .Where(x =>
                    x.Selected &&
                    x.Action.Equals(
                        "MOVE",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (operationsToApply.Count == 0)
        {
            MessageBox.Show(
                "There are no selected files or folders to move.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var fileCount = operationsToApply.Count(x => !x.IsDirectory);
        var folderCount = operationsToApply.Count(x => x.IsDirectory);

        var confirmation =
            MessageBox.Show(
                "You are about to move:\n" +
                $"{fileCount:N0} file(s)\n" +
                $"{folderCount:N0} folder(s), including their contents.\n\n" +
                "Existing destination files will NOT be overwritten.\n" +
                "Nothing will be deleted.\n\n" +
                "Continue?",
                "Confirm Organization",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        ApplyButton.IsEnabled = false;
        ScanButton.IsEnabled = false;

        var rootFolder =
            FolderPathTextBox.Text.Trim();

        var successfulMoves = 0;
        var failedMoves = 0;

        long runId = 0;

        try
        {
            runId =
                await _databaseService.StartRunAsync(
                    rootFolder,
                    _operations.Count,
                    operationsToApply.Count);

            for (var index = 0;
                 index < operationsToApply.Count;
                 index++)
            {
                var operation =
                    operationsToApply[index];

                StatusTextBlock.Text =
                    $"Moving {index + 1:N0} of " +
                    $"{operationsToApply.Count:N0}: " +
                    $"{operation.Name}";

                var originalSource =
                    operation.SourcePath;

                try
                {
                    var finalDestination =
                        await Task.Run(() =>
                            _fileMoveService.Move(operation));

                    successfulMoves++;

                    await _databaseService.RecordMoveAsync(
                        runId,
                        originalSource,
                        operation.DestinationPath,
                        finalDestination,
                        operation.Type,
                        "SUCCESS");
                }
                catch (Exception ex)
                {
                    failedMoves++;

                    await _databaseService.RecordMoveAsync(
                        runId,
                        originalSource,
                        operation.DestinationPath,
                        null,
                        operation.Type,
                        "FAILED",
                        ex.Message);
                }
            }

            await _databaseService.CompleteRunAsync(
                runId,
                successfulMoves,
                failedMoves);

            MessageBox.Show(
                $"Organization complete.\n\n" +
                $"Moved successfully: {successfulMoves:N0}\n" +
                $"Failed: {failedMoves:N0}\n\n" +
                "No files were deleted.",
                "Folder Organizer",
                MessageBoxButton.OK,
                failedMoves == 0
                    ? MessageBoxImage.Information
                    : MessageBoxImage.Warning);

            await RefreshScanAsync();
        }
        catch (Exception ex)
        {
            if (runId != 0)
            {
                try
                {
                    await _databaseService.CompleteRunAsync(
                        runId,
                        successfulMoves,
                        failedMoves);
                }
                catch
                {
                    // Preserve the original exception.
                }
            }

            MessageBox.Show(
                $"The organization run could not be completed.\n\n" +
                ex.Message,
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ScanButton.IsEnabled = true;
        }
    }

    private async void ScanButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var folder = FolderPathTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(folder))
        {
            MessageBox.Show(
                "Select a folder first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (!Directory.Exists(folder))
        {
            MessageBox.Show(
                "The selected folder does not exist.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        ScanButton.IsEnabled = false;
        ApplyButton.IsEnabled = false;

        try
        {
            await RefreshScanAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Scan failed:\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            StatusTextBlock.Text = "Scan failed.";
        }
        finally
        {
            ScanButton.IsEnabled = true;
        }
    }
}
