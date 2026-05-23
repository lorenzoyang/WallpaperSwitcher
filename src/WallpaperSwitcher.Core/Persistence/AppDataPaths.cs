namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Provides the application-owned paths used to store user data.
/// </summary>
/// <remarks>
/// Keeping these paths in one place ensures all persisted user files are written under the same
/// <c>WallpaperSwitcher</c> directory in the current user's local application data folder.
/// </remarks>
public static class AppDataPaths
{
    /// <summary>
    /// Gets the root directory used for Wallpaper Switcher user data.
    /// </summary>
    public static string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperSwitcher"
    );

    /// <summary>
    /// Gets the full path of the JSON file used to persist global hotkey bindings.
    /// </summary>
    public static string HotkeysFile => Path.Combine(RootDirectory, "hotkeys.json");

    /// <summary>
    /// Gets the full path of the JSON file used to persist application settings.
    /// </summary>
    public static string SettingsFile => Path.Combine(RootDirectory, "settings.json");
}
