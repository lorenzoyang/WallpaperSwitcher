namespace WallpaperSwitcher.Core.Persistence;

public static class AppDataPaths
{
    public static string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperSwitcher"
    );

    public static string HotkeysFile => Path.Combine(RootDirectory, "hotkeys.json");

    public static string SettingsFile => Path.Combine(RootDirectory, "settings.json");
}