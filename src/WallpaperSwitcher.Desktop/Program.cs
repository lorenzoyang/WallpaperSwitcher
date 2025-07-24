using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WallpaperSwitcher.Desktop
{
    internal static class Program
    {
        private const string MutexName = "WallpaperSwitcher.Desktop.UniqueInstance"; // Use a unique name

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Create or open existing mutex
            using var mutex = new Mutex(true, MutexName, out var isFirstInstance);
            if (!isFirstInstance) return;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}