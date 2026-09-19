using System.Windows;
using System.Windows.Media;

namespace FolderOrganizer.Services;

public static class ThemeService
{
    public static bool IsDarkMode { get; private set; }

    public static void Apply(bool darkMode)
    {
        IsDarkMode = darkMode;

        var resources =
            Application.Current.Resources;

        if (darkMode)
        {
            // Main application background
            resources["WindowBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(25, 29, 36));

            // Raised panels / DataGrid backgrounds
            resources["PanelBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(34, 40, 50));

            // Buttons and general controls
            resources["ControlBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(32, 37, 45));

            // Headers / text input backgrounds
            resources["HeaderBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(24, 28, 35));

            // Primary text
            resources["TextBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(245, 247, 250));

            // Secondary / descriptive text
            resources["SecondaryTextBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(190, 200, 212));

            // Borders and separators
            resources["BorderBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(58, 70, 85));

            // Selected rows / items
            resources["SelectionBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(18, 95, 120));

            // Hovered controls
            resources["HoverBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(41, 49, 60));

            // Cyan accent matching Dowe LanCaster
            resources["AccentBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(0, 168, 216));
        }
        else
        {
            resources["WindowBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(245, 245, 245));

            resources["PanelBackgroundBrush"] =
                new SolidColorBrush(
                    Colors.White);

            resources["ControlBackgroundBrush"] =
                new SolidColorBrush(
                    Colors.White);

            resources["HeaderBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(240, 240, 240));

            resources["TextBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(31, 31, 31));

            resources["SecondaryTextBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(102, 102, 102));

            resources["BorderBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(216, 216, 216));

            resources["SelectionBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(221, 216, 255));

            resources["HoverBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(238, 238, 238));

            resources["AccentBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(103, 80, 216));
        }
    }
}
