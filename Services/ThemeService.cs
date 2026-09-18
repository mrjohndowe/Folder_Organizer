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
            resources["WindowBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(18, 18, 18));

            resources["PanelBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(30, 30, 30));

            resources["ControlBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(37, 37, 38));

            resources["HeaderBackgroundBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(45, 45, 48));

            resources["TextBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(242, 242, 242));

            resources["SecondaryTextBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(183, 183, 183));

            resources["BorderBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(70, 70, 74));

            resources["SelectionBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(58, 58, 64));

            resources["HoverBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(51, 51, 55));

            resources["AccentBrush"] =
                new SolidColorBrush(
                    Color.FromRgb(124, 106, 230));
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