using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace FolderOrganizer.Views
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            Loaded += SplashScreen_Loaded;
        }

        private async void SplashScreen_Loaded(
    object sender,
    RoutedEventArgs e)
        {
            Storyboard transferStoryboard =
                (Storyboard)FindResource(
                    "FileTransferStoryboard");

            transferStoryboard.Begin(this, true);

            DateTime splashStarted =
                DateTime.UtcNow;

            try
            {
                await RunStartupSequence();

                // Keep the splash visible long enough for the
                // transfer animation to actually be seen.
                const int minimumSplashTime = 30000;

                int elapsed =
                    (int)(DateTime.UtcNow - splashStarted)
                        .TotalMilliseconds;

                int remaining =
                    minimumSplashTime - elapsed;

                if (remaining > 0)
                {
                    await Task.Delay(remaining);
                }

                transferStoryboard.Stop(this);

                MainWindow mainWindow =
                    new MainWindow();

                mainWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                transferStoryboard.Stop(this);

                MessageBox.Show(
                    $"Folder Organizer failed to start.\n\n{ex.Message}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
        }

        private async Task RunStartupSequence()
        {
            await SetProgress(
                10,
                "Checking application folders..."
            );

            EnsureApplicationFolders();

            await SetProgress(
                30,
                "Checking database..."
            );

            // Database initialization will go here.

            await SetProgress(
                50,
                "Loading settings..."
            );

            // Settings loading will go here.

            await SetProgress(
                70,
                "Loading organizer rules..."
            );

            // Organizer rules initialization will go here.

            await SetProgress(
                90,
                "Preparing workspace..."
            );

            // Workspace initialization will go here.

            await SetProgress(
                100,
                "Ready"
            );
        }

        private async Task SetProgress(int percentage, string status)
        {
            StatusText.Text = status;
            StartupProgress.Value = percentage;
            PercentageText.Text = $"{percentage}%";

            await Task.Delay(350);
        }

        private void EnsureApplicationFolders()
        {
            string appData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );

            string rootFolder = Path.Combine(
                appData,
                "FolderOrganizer"
            );

            string databaseFolder = Path.Combine(
                rootFolder,
                "Database"
            );

            string logsFolder = Path.Combine(
                rootFolder,
                "Logs"
            );

            string backupsFolder = Path.Combine(
                rootFolder,
                "Backups"
            );

            Directory.CreateDirectory(rootFolder);
            Directory.CreateDirectory(databaseFolder);
            Directory.CreateDirectory(logsFolder);
            Directory.CreateDirectory(backupsFolder);
        }
    }
}