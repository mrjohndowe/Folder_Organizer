using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FolderOrganizer.Models;
using FolderOrganizer.Services;

namespace FolderOrganizer;

public partial class HistoryWindow : Window
{
    private readonly DatabaseService _databaseService;

    private readonly ObservableCollection<OrganizationRun>
        _runs = [];

    private readonly ObservableCollection<MoveHistoryEntry>
        _moves = [];

    public HistoryWindow(
        DatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        RunsDataGrid.ItemsSource = _runs;
        MovesDataGrid.ItemsSource = _moves;

        Loaded += HistoryWindow_Loaded;
    }

    private async void HistoryWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRunsAsync();
    }

    private async Task LoadRunsAsync()
    {
        _runs.Clear();
        _moves.Clear();

        StatusTextBlock.Text =
            "Loading history...";

        try
        {
            var runs =
                await _databaseService.GetRunsAsync();

            foreach (var run in runs)
            {
                _runs.Add(run);
            }

            StatusTextBlock.Text =
                $"{_runs.Count:N0} organization runs";

            if (_runs.Count > 0)
            {
                RunsDataGrid.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"History could not be loaded.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            StatusTextBlock.Text =
                "History load failed.";
        }
    }

    private async void RunsDataGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (RunsDataGrid.SelectedItem
            is not OrganizationRun run)
        {
            return;
        }

        _moves.Clear();

        try
        {
            var entries =
                await _databaseService
                    .GetMoveHistoryAsync(run.Id);

            foreach (var entry in entries)
            {
                _moves.Add(entry);
            }

            StatusTextBlock.Text =
                $"Run {run.Id}: " +
                $"{_moves.Count:N0} file records";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Run details could not be loaded.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRunsAsync();
    }

    private async void UndoSelectedMoveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (MovesDataGrid.SelectedItem is not MoveHistoryEntry entry)
        {
            MessageBox.Show(
                "Select a successful move from the history first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (!entry.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(entry.FinalDestinationPath))
        {
            MessageBox.Show(
                "Only successful moves that have not already been undone " +
                "can be restored.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var isFolder = entry.FileType.Equals(
            "Folder",
            StringComparison.OrdinalIgnoreCase);
        var movedItemExists = isFolder
            ? Directory.Exists(entry.FinalDestinationPath)
            : File.Exists(entry.FinalDestinationPath);

        if (!movedItemExists)
        {
            MessageBox.Show(
                "The moved item is no longer at the location recorded in " +
                "history, so it cannot be safely undone.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (File.Exists(entry.SourcePath) || Directory.Exists(entry.SourcePath))
        {
            MessageBox.Show(
                "The original location is already occupied. Undo will not " +
                "overwrite an existing file or folder.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            $"Move this {(isFolder ? "folder and all of its contents" : "file")} " +
            "back to its original location?\n\n" +
            "Existing items will not be overwritten.",
            "Undo Move",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var sourceDirectory = Path.GetDirectoryName(entry.SourcePath);

            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new InvalidOperationException(
                    "The original location is invalid.");
            }

            Directory.CreateDirectory(sourceDirectory);

            if (isFolder)
            {
                Directory.Move(entry.FinalDestinationPath, entry.SourcePath);
            }
            else
            {
                File.Move(entry.FinalDestinationPath, entry.SourcePath);
            }

            await _databaseService.MarkMoveUndoneAsync(entry.Id);

            entry.Status = "UNDONE";
            entry.ErrorMessage = null;
            MovesDataGrid.Items.Refresh();
            StatusTextBlock.Text = "Move undone successfully.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The move could not be undone.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}
