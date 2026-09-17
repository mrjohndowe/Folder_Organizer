using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FolderOrganizer.Models;
using FolderOrganizer.Services;
using Microsoft.Win32;
using System.Windows.Controls;

namespace FolderOrganizer;

public partial class MainWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly ObservableCollection<MoveOperation> _operations = [];
    private readonly DestinationService _destinationService;
    private readonly FileMoveService _fileMoveService;

    private readonly ClassificationService _classificationService;
    private readonly ScanService _scanService;

    public MainWindow()
    {
        InitializeComponent();

        _classificationService = new ClassificationService();
        _scanService = new ScanService(_classificationService);
        _databaseService = new DatabaseService();
        _destinationService = new DestinationService();
        _fileMoveService = new FileMoveService(_destinationService);

        Loaded += MainWindow_Loaded;

        PreviewDataGrid.ItemsSource = _operations;
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

    private async void MainWindow_Loaded(
    object sender,
    RoutedEventArgs e)
    {
        try
        {
            await _databaseService.InitializeAsync();
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

    private async Task RefreshScanAsync()
    {
        var folder = FolderPathTextBox.Text.Trim();

        if (!Directory.Exists(folder))
        {
            return;
        }

        _operations.Clear();

        StatusTextBlock.Text = "Scanning...";

        var results =
            await _scanService.ScanAsync(
                folder,
                IncludeSubfoldersCheckBox.IsChecked == true);

        foreach (var result in results)
        {
            _operations.Add(result);
        }

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

        ApplyButton.IsEnabled =
            _operations.Any(x =>
                x.Selected &&
                x.Action.Equals(
                    "MOVE",
                    StringComparison.OrdinalIgnoreCase));
    }

    private async void ApplyButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        PreviewDataGrid.CommitEdit(
            DataGridEditingUnit.Cell,
            true);

        PreviewDataGrid.CommitEdit(
            DataGridEditingUnit.Row,
            true);

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
                "There are no selected files to move.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var confirmation =
            MessageBox.Show(
                $"You are about to move " +
                $"{operationsToApply.Count:N0} files.\n\n" +
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