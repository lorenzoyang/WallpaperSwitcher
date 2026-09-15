namespace WallpaperSwitcher.Desktop;

internal enum ModernMenuIcon
{
    None,
    Folder,
    NextWallpaper,
    Settings,
    Exit
}

internal sealed class ModernMenuItem : ToolStripMenuItem
{
    public ModernMenuItem(string text, ModernMenuIcon icon = ModernMenuIcon.None, EventHandler? onClick = null)
        : base(text, null, onClick)
    {
        MenuIcon = icon;
        ShowShortcutKeys = false;
    }

    public ModernMenuIcon MenuIcon { get; }
}
