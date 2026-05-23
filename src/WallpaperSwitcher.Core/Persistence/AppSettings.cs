namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Represents the user-configurable application settings persisted by Wallpaper Switcher.
/// </summary>
/// <remarks>
/// These settings are stored in <c>settings.json</c>. Hotkey bindings are stored separately in
/// <c>hotkeys.json</c>.
/// </remarks>
public sealed class AppSettings
{
    /// <summary>
    /// Ensures loaded settings have non-null collection and string values.
    /// </summary>
    /// <returns>The current settings instance after default values are restored where needed.</returns>
    internal AppSettings Normalize()
    {
        WallpaperFolders ??= [];
        LastSelectedFolder ??= string.Empty;
        return this;
    }

    /// <summary>
    /// Gets or sets the wallpaper folders configured by the user.
    /// </summary>
    public List<string> WallpaperFolders { get; set; } = [];

    /// <summary>
    /// Gets or sets the folder that was selected when the application last saved settings.
    /// </summary>
    public string LastSelectedFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected wallpaper switching mode index.
    /// </summary>
    /// <remarks>
    /// The desktop UI currently interprets <c>0</c> as native Windows slideshow mode and
    /// <c>1</c> as custom fast switching mode.
    /// </remarks>
    public int SelectedModeIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the system tray hint has already been shown.
    /// </summary>
    public bool HasShownTrayTip { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the application should launch when Windows starts.
    /// </summary>
    public bool LaunchAtStartup { get; set; }
}
