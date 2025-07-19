namespace WallpaperSwitcher.Core;

public class AppConfig
{
    public List<string> WallpaperFolders { get; set; } = new List<string>();
    public string NextHotkey { get; set; } = "Ctrl+Shift+N";
    public string PreviousHotkey { get; set; } = "Ctrl+Shift+P";
    public bool RunOnStartup { get; set; } = true;
}