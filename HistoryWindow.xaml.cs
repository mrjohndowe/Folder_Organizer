using System.Collections.ObjectModel;
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

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}