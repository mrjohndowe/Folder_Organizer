using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using FolderOrganizer.Models;
using FolderOrganizer.Services;
using System.Windows.Input;
using System.Windows.Media;

namespace FolderOrganizer;

public partial class SettingsWindow : Window
{
    private readonly DatabaseService _databaseService;

    private readonly ObservableCollection<CustomRule> _rules = [];

    private Point _dragStartPoint;

    private CustomRule? _draggedRule;

    private bool _isLoading;

    public SettingsWindow(
        DatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        RulesDataGrid.ItemsSource = _rules;

        Loaded += SettingsWindow_Loaded;
    }

    private void RulesDataGrid_PreviewMouseLeftButtonDown(
    object sender,
    MouseButtonEventArgs e)
    {
        _dragStartPoint =
            e.GetPosition(null);

        _draggedRule =
            FindRuleFromSource(
                e.OriginalSource as DependencyObject);
    }

    private void RulesDataGrid_PreviewMouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed ||
            _draggedRule == null)
        {
            return;
        }

        var currentPosition =
            e.GetPosition(null);

        var difference =
            _dragStartPoint - currentPosition;

        if (Math.Abs(difference.X) <
                SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(difference.Y) <
                SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        DragDrop.DoDragDrop(
            RulesDataGrid,
            _draggedRule,
            DragDropEffects.Move);
    }

    private async void RulesDataGrid_Drop(
        object sender,
        DragEventArgs e)
    {
        if (_draggedRule == null)
        {
            return;
        }

        var targetRule =
            FindRuleFromSource(
                e.OriginalSource as DependencyObject);

        if (targetRule == null ||
            targetRule.Id == _draggedRule.Id)
        {
            _draggedRule = null;
            return;
        }

        try
        {
            await _databaseService.MoveCustomRuleAsync(
                _draggedRule.Id,
                targetRule.Priority);

            await LoadRulesAsync();

            StatusTextBlock.Text =
                "Rule order autosaved.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The rule could not be reordered.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            await LoadRulesAsync();
        }
        finally
        {
            _draggedRule = null;
        }
    }

    private CustomRule? FindRuleFromSource(
        DependencyObject? source)
    {
        while (source != null)
        {
            if (source is DataGridRow row &&
                row.Item is CustomRule rule)
            {
                return rule;
            }

            source =
                VisualTreeHelper.GetParent(source);
        }

        return null;
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

    private async void DeleteRuleButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (RulesDataGrid.SelectedItem
            is not CustomRule rule)
        {
            MessageBox.Show(
                "Select a custom rule first.",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var confirmation =
            MessageBox.Show(
                $"Delete the custom rule \"{rule.FolderName}\"?\n\n" +
                "This removes the rule only. " +
                "It will not delete or move any files.",
                "Delete Custom Rule",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _databaseService.DeleteCustomRuleAsync(
                rule.Id);

            await LoadRulesAsync();

            StatusTextBlock.Text =
                $"Deleted custom rule: {rule.FolderName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The custom rule could not be deleted.\n\n{ex.Message}",
                "Folder Organizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

            await LoadRulesAsync();
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

                await LoadRulesAsync();
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