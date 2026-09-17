using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using FolderOrganizer.Models;
using FolderOrganizer.Services;

namespace FolderOrganizer;

public partial class SettingsWindow : Window
{
    private readonly DatabaseService _databaseService;

    private readonly ObservableCollection<CustomRule> _rules = [];

    private bool _isLoading;

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

    private async Task LoadRulesAsync()
    {
        _isLoading = true;

        try
        {
            _rules.Clear();

            StatusTextBlock.Text =
                "Loading custom rules...";

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
        finally
        {
            _isLoading = false;
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

    private async void EnabledCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (sender is not CheckBox checkBox ||
            checkBox.DataContext is not CustomRule rule)
        {
            return;
        }

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

    private void RulesDataGrid_CellEditEnding(
    object sender,
    DataGridCellEditEndingEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (e.Row.Item is not CustomRule rule)
        {
            return;
        }

        // Priority has its own save/reorder handler.
        // Do NOT let the normal autosave overwrite it.
        if (e.Column == PriorityColumn)
        {
            return;
        }

        if (e.EditingElement is TextBox textBox)
        {
            textBox
                .GetBindingExpression(TextBox.TextProperty)?
                .UpdateSource();
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

    private async void PriorityTextBox_LostFocus(
        object sender,
        RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (sender is not TextBox textBox ||
            textBox.DataContext is not CustomRule rule)
        {
            return;
        }

        if (!int.TryParse(
                textBox.Text.Trim(),
                out var requestedPriority))
        {
            MessageBox.Show(
                "Priority must be a whole number.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadRulesAsync();
            return;
        }

        try
        {
            await _databaseService.MoveCustomRuleAsync(
                rule.Id,
                requestedPriority);

            await LoadRulesAsync();

            StatusTextBlock.Text =
                "Rule priority autosaved.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The rule priority could not be saved.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            await LoadRulesAsync();
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        RulesDataGrid.CommitEdit(
            DataGridEditingUnit.Cell,
            true);

        RulesDataGrid.CommitEdit(
            DataGridEditingUnit.Row,
            true);

        Close();
    }
}