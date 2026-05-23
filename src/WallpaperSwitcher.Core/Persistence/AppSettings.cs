namespace WallpaperSwitcher.Core.Persistence;

public sealed class AppSettings
{
    public List<string> WallpaperFolders { get; set; } = [];

    public string LastSelectedFolder { get; set; } = string.Empty;

    public int SelectedModeIndex { get; set; }

    public bool HasShownTrayTip { get; set; }

    public bool LaunchAtStartup { get; set; }
}