using System.Reflection;

public static class AppVersion
{
    public static string Current
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return version == null
                ? "0.0.0"
                : $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}