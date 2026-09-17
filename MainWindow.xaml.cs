using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FolderOrganizer.Models;
using FolderOrganizer.Services;
using Microsoft.Win32;

namespace FolderOrganizer;

public partial class MainWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly ObservableCollection<MoveOperation> _operations = [];

    private readonly ClassificationService _classificationService;
    private readonly ScanService _scanService;

    public MainWindow()
    {
        InitializeComponent();

        _classificationService = new ClassificationService();
        _scanService = new ScanService(_classificationService);
        _databaseService = new DatabaseService();

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

        StatusTextBlock.Text = "Scanning...";

        _operations.Clear();

        try
        {
            var includeSubfolders =
                IncludeSubfoldersCheckBox.IsChecked == true;

            var results =
                await _scanService.ScanAsync(
                    folder,
                    includeSubfolders);

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

            /*
             * Intentionally disabled for now.
             *
             * Scanning/previewing cannot modify files.
             * We'll enable this only after FileMoveService,
             * collision protection, and SQLite history are wired up.
             */
            ApplyButton.IsEnabled = false;
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
