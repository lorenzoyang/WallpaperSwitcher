using System.Diagnostics;

namespace WallpaperSwitcher.Desktop
{
    internal static class Program
    {
        // Use a unique name
        private const string MutexName = @"Global\WallpaperSwitcher.Desktop.UniqueInstance.Name";

        private const string AppName = "Wallpaper Switcher";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Create or open existing mutex
                using var mutex = new Mutex(true, MutexName, out var isFirstInstance);
                if (!isFirstInstance)
                {
                    // Try to activate existing instance
                    var result = FormHelper.TryActivateExistingInstance(AppName);
                    if (result)
                    {
                        return;
                    }

                    // If activation failed
                    throw new InvalidOperationException(
                        "Another instance of the application is already running, but it could not be activated."
                    );
                }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle case where mutex exists, but we can't access it
                Debug.WriteLine($"Mutex access denied: {ex.Message}");
                FormHelper.ShowWarningMessage(
                    "Another instance of the application is running with different permissions.",
                    "Access Denied"
                );
            }
            catch (Exception ex)
            {
                // Handle any other startup exceptions
                Debug.WriteLine($"Startup error: {ex}");
                FormHelper.ShowErrorMessage(
                    $"Failed to start application: {ex.Message}",
                    "Startup Error"
                );
            }
        }
    }
}