using System.Reflection;
using Microsoft.Win32;

namespace WallpaperSwitcher.Core;

/// <summary>
/// 
/// </summary>
public static class StartupManager
{
    /// <summary>
    /// 
    /// </summary>
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// 
    /// </summary>
    private static string ApplicationName => Assembly.GetEntryAssembly()?.GetName().Name ?? "WallpaperSwitcher";

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
    /// 
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="startMinimized"></param>
    /// <returns></returns>
    public static bool SetStartupEnabled(bool enable, bool startMinimized = true)
    {
        return enable ? RegisterForStartup(startMinimized) : UnregisterFromStartup();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startMinimized"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// 
    /// </summary>
    /// <returns></returns>
    private static bool UnregisterFromStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        key?.DeleteValue(ApplicationName, false);
        return true;
    }
}