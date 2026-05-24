namespace WallpaperSwitcher.Desktop;

internal static class Program
{
    // Use a global mutex name so elevated and non-elevated instances do not run side by side.
    private const string MutexName = @"Global\WallpaperSwitcher.Desktop.UniqueInstance.Name";

    private const string AppName = "Wallpaper Switcher";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        bool startMinimized = args.Contains("--minimized", StringComparer.OrdinalIgnoreCase);
        try
        {
            using var mutex = new Mutex(true, MutexName, out var isFirstInstance);
            if (!isFirstInstance)
            {
                var result = FormHelper.TryActivateExistingInstance(AppName);
                if (result)
                {
                    return;
                }

                throw new InvalidOperationException(
                    "Another instance of the application is already running, but it could not be activated."
                );
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(startMinimized));
        }
        catch (UnauthorizedAccessException ex)
        {
            FormHelper.ShowWarningMessage(
                $"Another instance of the application is running with different permissions: {ex.Message}",
                "Access Denied"
            );
        }
        catch (Exception ex)
        {
            FormHelper.ShowErrorMessageWithLink(
                $"Failed to start application: {ex.Message}",
                "Startup Error"
            );
        }
    }
}
