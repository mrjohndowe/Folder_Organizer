using System.Collections.ObjectModel;
using System.Windows;
using FolderOrganizer.Models;
using FolderOrganizer.Services;
using System.Windows.Controls;

namespace FolderOrganizer;

public partial class SettingsWindow : Window
{
    private readonly DatabaseService _databaseService;

    private readonly ObservableCollection<CustomRule>
        _rules = [];

    public SettingsWindow(
        DatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        RulesDataGrid.ItemsSource = _rules;

        Loaded += SettingsWindow_Loaded;
    }

    private async void SettingsWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRulesAsync();
    }

    private async void EnabledCheckBox_Changed(
    object sender,
    RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox ||
            checkBox.DataContext is not CustomRule rule)
        {
            return;
        }

        // Explicitly use the checkbox's current value.
        rule.IsEnabled =
            checkBox.IsChecked == true;

        try
        {
            await _databaseService.UpdateCustomRuleAsync(rule);

            StatusTextBlock.Text =
                $"Autosaved: {rule.FolderName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The custom rule could not be saved.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            StatusTextBlock.Text =
                "Autosave failed.";
        }
    }

    private async void AddRuleButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        try
        {
            var newRuleId =
                await _databaseService.AddCustomRuleAsync(
                    "New Folder",
                    string.Empty);

            await LoadRulesAsync();

            var newRule =
                _rules.FirstOrDefault(
                    x => x.Id == newRuleId);

            if (newRule != null)
            {
                RulesDataGrid.SelectedItem = newRule;
                RulesDataGrid.ScrollIntoView(newRule);
            }

            StatusTextBlock.Text =
                "Custom rule added.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The custom rule could not be added.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void RulesDataGrid_CellEditEnding(
    object sender,
    DataGridCellEditEndingEventArgs e)
    {
        if (e.Row.Item is not CustomRule rule)
        {
            return;
        }

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                await _databaseService.UpdateCustomRuleAsync(rule);

                StatusTextBlock.Text =
                    $"Autosaved: {rule.FolderName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The custom rule could not be saved.\n\n{ex.Message}",
                    "Folder Organizer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                StatusTextBlock.Text =
                    "Autosave failed.";
            }
        });
    }

    private async Task LoadRulesAsync()
    {
        _rules.Clear();

        StatusTextBlock.Text =
            "Loading custom rules...";

        try
        {
            var rules =
                await _databaseService.GetCustomRulesAsync();

            foreach (var rule in rules)
            {
                _rules.Add(rule);
            }

            StatusTextBlock.Text =
                $"{_rules.Count:N0} custom rules";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Custom rules could not be loaded.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            StatusTextBlock.Text =
                "Could not load custom rules.";
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}