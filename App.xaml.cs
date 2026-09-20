using System.Windows;
using AppSplashScreen = FolderOrganizer.Views.SplashScreen;

namespace FolderOrganizer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppSplashScreen splash = new AppSplashScreen();
            splash.Show();
        }
    }
}