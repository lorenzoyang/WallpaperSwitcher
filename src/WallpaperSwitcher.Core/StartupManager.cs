using System.Reflection;
using Microsoft.Win32;

namespace WallpaperSwitcher.Core;

/// <summary>
/// Manages the current user's Windows startup registration for Wallpaper Switcher.
/// </summary>
/// <remarks>
/// Startup state is stored under the per-user <c>Run</c> registry key, so enabling startup does
/// not require administrator privileges in normal Windows configurations.
/// </remarks>
public static class StartupManager
{
    /// <summary>
    /// The per-user registry key where Windows reads startup commands.
    /// </summary>
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Gets the registry value name used for this application.
    /// </summary>
    private static string ApplicationName => Assembly.GetEntryAssembly()?.GetName().Name ?? "WallpaperSwitcher";

    /// <summary>
    /// Gets the executable path that should be launched by Windows at sign-in.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current process path cannot be resolved to an executable file.
    /// </exception>
    private static string ExecutablePath
    {
        get
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exePath) || !exePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Failed to determine the correct executable path.");
            }

            return exePath;
        }
    }

    /// <summary>
    /// Enables or disables launch at Windows startup for the current user.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to register the app for startup; <see langword="false"/> to remove it.
    /// </param>
    /// <param name="startMinimized">
    /// <see langword="true"/> to add the <c>--minimized</c> startup argument.
    /// </param>
    /// <returns><see langword="true"/> when the requested registry update completes.</returns>
    public static bool SetStartupEnabled(bool enable, bool startMinimized = true)
    {
        return enable ? RegisterForStartup(startMinimized) : UnregisterFromStartup();
    }

    /// <summary>
    /// Writes the startup command to the current user's Run registry key.
    /// </summary>
    /// <param name="startMinimized">Whether Windows should launch the app minimized to the tray.</param>
    /// <returns><see langword="true"/> when registration succeeds.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the startup registry key or executable path cannot be resolved.
    /// </exception>
    private static bool RegisterForStartup(bool startMinimized = true)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        if (key == null)
        {
            throw new InvalidOperationException("Unable to access Windows startup registry key");
        }

        // Add --minimized argument so the app starts directly to system tray
        var commandLine = startMinimized ? $"\"{ExecutablePath}\" --minimized" : $"\"{ExecutablePath}\"";
        key.SetValue(ApplicationName, commandLine);

        return true;
    }

    /// <summary>
    /// Removes the startup command from the current user's Run registry key.
    /// </summary>
    /// <returns><see langword="true"/> after the value has been removed or was already absent.</returns>
    private static bool UnregisterFromStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        key?.DeleteValue(ApplicationName, false);
        return true;
    }
}
