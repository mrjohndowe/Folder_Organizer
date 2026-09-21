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

    private async void UndoSelectedRunButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (RunsDataGrid.SelectedItem is not OrganizationRun run)
        {
            MessageBox.Show(
                "Select an organization run first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var entries = (await _databaseService.GetMoveHistoryAsync(run.Id))
            .Where(x => x.Status.Equals(
                "SUCCESS",
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Id)
            .ToList();

        if (entries.Count == 0)
        {
            MessageBox.Show(
                "This run has no successful moves that can be undone.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var validationError = ValidateUndoEntries(entries);

        if (validationError != null)
        {
            MessageBox.Show(
                "This run cannot be safely undone. No items were moved.\n\n" +
                validationError,
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            $"Undo run {run.Id} and restore {entries.Count:N0} item(s) " +
            "to their original locations?\n\n" +
            "Existing items will not be overwritten.",
            "Undo Organization Run",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            foreach (var entry in entries)
            {
                var sourceDirectory = Path.GetDirectoryName(entry.SourcePath)
                    ?? throw new InvalidOperationException(
                        "The original location is invalid.");

                Directory.CreateDirectory(sourceDirectory);

                if (IsFolder(entry))
                {
                    Directory.Move(entry.FinalDestinationPath!, entry.SourcePath);
                }
                else
                {
                    File.Move(entry.FinalDestinationPath!, entry.SourcePath);
                }
            }

            await _databaseService.MarkMovesUndoneAsync(
                entries.Select(x => x.Id));

            await LoadRunsAsync();
            StatusTextBlock.Text =
                $"Run {run.Id} undone successfully: " +
                $"{entries.Count:N0} item(s) restored.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The run could not be undone.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string? ValidateUndoEntries(
        IEnumerable<MoveHistoryEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.FinalDestinationPath))
            {
                return $"{entry.SourcePath} has no recorded final location.";
            }

            var movedItemExists = IsFolder(entry)
                ? Directory.Exists(entry.FinalDestinationPath)
                : File.Exists(entry.FinalDestinationPath);

            if (!movedItemExists)
            {
                return "A moved item is no longer at its recorded location: " +
                       entry.FinalDestinationPath;
            }

            if (File.Exists(entry.SourcePath) ||
                Directory.Exists(entry.SourcePath))
            {
                return "An original location is already occupied: " +
                       entry.SourcePath;
            }
        }

        return null;
    }

    private static bool IsFolder(MoveHistoryEntry entry) =>
        entry.FileType.Equals("Folder", StringComparison.OrdinalIgnoreCase);

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}
