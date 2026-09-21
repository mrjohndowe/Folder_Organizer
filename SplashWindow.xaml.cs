using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FolderOrganizer.Services;
using System.Reflection;


namespace FolderOrganizer.Views
{
    public partial class SplashScreen : Window
    {
        private readonly DispatcherTimer _quipTimer = new DispatcherTimer();
        private readonly Random _random = new Random();

        private int _lastQuipIndex = -1;

        private readonly string[] _loadingQuips =
        {
            "Checking all your files... Calculating how much porn you have.",
            "Loading your images... Checking if you have any nudes.",
            "Looking for a folder named Definitely_Not_Porn...",
            "Checking if 'Homework' is actually homework.",
            "Counting screenshots you swore you'd delete later.",
            "Inspecting your Downloads folder... This may take a while.",
            "Judging your file naming conventions silently.",
            "Searching for final_final_REALLY_FINAL_v2.docx...",
            "Trying to understand why everything is on the Desktop.",
            "Separating important files from digital archaeology.",
            "Looking for duplicate files... Found approximately all of them.",
            "Checking whether New Folder (17) was truly necessary.",
            "Organizing years of questionable decisions.",
            "Looking for files you downloaded and immediately forgot about.",
            "Counting memes that are technically historical documents now.",
            "Checking how many screenshots could have been bookmarks.",
            "Investigating the mysterious purpose of Miscellaneous.",
            "Searching for tax documents among the memes.",
            "Checking whether your Documents folder contains documents.",
            "Examining filenames only a past version of you understands.",
            "Looking for abandoned ZIP files.",
            "Calculating how much storage is dedicated to things you'll never open.",
            "Trying not to ask why there are twelve copies of the same PDF.",
            "Sorting files. Judging nothing. Mostly.",
            "Checking Recycle Bin escape attempts.",
            "Locating ancient installers from computers you no longer own.",
            "Trying to determine what 'stuff2' was supposed to contain.",
            "Searching for passwords.txt... kidding. Mostly.",
            "Consulting the sacred laws of alphabetical order.",
            "Making your filesystem look like an adult lives here."
        };

        public SplashScreen()
        {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            if (version != null)
            {
                FooterVersionText.Text =
                    FooterVersionText.Text = $"Folder Organizer  •  v{AppVersion.Current}";
            }

            _quipTimer.Interval = TimeSpan.FromMilliseconds(3500);
            _quipTimer.Tick += QuipTimer_Tick;

            Loaded += SplashScreen_Loaded;
        }

        private void QuipTimer_Tick(
            object? sender,
            EventArgs e)
        {
            int index;

            do
            {
                index = _random.Next(_loadingQuips.Length);
            }
            while (
                index == _lastQuipIndex &&
                _loadingQuips.Length > 1);

            _lastQuipIndex = index;

            QuipText.Text = _loadingQuips[index];
        }

        private async void SplashScreen_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            _quipTimer.Start();

            // Show the first quip immediately.
            QuipTimer_Tick(null, EventArgs.Empty);

            Storyboard? transferStoryboard = null;

            DateTime splashStarted = DateTime.UtcNow;

            try
            {
                transferStoryboard =
                    FindResource("FileTransferStoryboard")
                    as Storyboard;

                if (transferStoryboard != null)
                {
                    transferStoryboard.Begin(
                        this,
                        true);
                }

                await RunStartupSequence();

                if (Application.Current.Dispatcher.HasShutdownStarted)
                {
                    return;
                }

                // Keep the splash visible long enough for the
                // animation and quips to actually be seen.
                const int minimumSplashTime = 8000;

                int elapsed =
                    (int)(DateTime.UtcNow - splashStarted)
                    .TotalMilliseconds;

                int remaining =
                    minimumSplashTime - elapsed;

                if (remaining > 0)
                {
                    await Task.Delay(remaining);
                }

                _quipTimer.Stop();

                if (transferStoryboard != null)
                {
                    transferStoryboard.Stop(this);
                }

                MainWindow mainWindow =
                    new MainWindow();

                mainWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                _quipTimer.Stop();

                if (transferStoryboard != null)
                {
                    transferStoryboard.Stop(this);
                }

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
                "Checking application folders...");

            EnsureApplicationFolders();

            await SetProgress(
                30,
                "Checking database...");

            // Database initialization will go here.

            await SetProgress(
                50,
                "Loading settings...");

            // Settings loading will go here.

            await SetProgress(
                70,
                "Loading organizer rules...");

            // Organizer rules initialization will go here.

            await SetProgress(
                85,
                "Preparing workspace...");

            // Workspace initialization will go here.

            await SetProgress(
                92,
                "Checking for updates...");

            await UpdateService.CheckForUpdatesAsync();

            // If the updater was launched, UpdateService shuts
            // down the application. Do not continue startup.
            if (Application.Current.Dispatcher.HasShutdownStarted)
            {
                return;
            }

            await SetProgress(
                100,
                "Ready");
        }

        private async Task SetProgress(
            int targetPercentage,
            string status)
        {
            StatusText.Text = status;

            int currentPercentage =
                (int)StartupProgress.Value;

            while (currentPercentage < targetPercentage)
            {
                currentPercentage++;

                StartupProgress.Value =
                    currentPercentage;

                PercentageText.Text =
                    $"{currentPercentage}%";

                // Smooth enough to see without making the
                // loading screen last until retirement.
                await Task.Delay(250);
            }

            // Leave each status on screen briefly so it can be read.
            await Task.Delay(250);
        }

        private void EnsureApplicationFolders()
        {
            string appData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

            string rootFolder =
                Path.Combine(
                    appData,
                    "FolderOrganizer");

            string databaseFolder =
                Path.Combine(
                    rootFolder,
                    "Database");

            string logsFolder =
                Path.Combine(
                    rootFolder,
                    "Logs");

            string backupsFolder =
                Path.Combine(
                    rootFolder,
                    "Backups");

            Directory.CreateDirectory(rootFolder);
            Directory.CreateDirectory(databaseFolder);
            Directory.CreateDirectory(logsFolder);
            Directory.CreateDirectory(backupsFolder);
        }
    }
}