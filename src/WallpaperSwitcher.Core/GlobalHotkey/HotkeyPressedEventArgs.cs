namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Provides data for the <see cref="HotkeyService.HotkeyPressed"/> event.
/// </summary>
/// <param name="hotkeyInfo">The information about the hotkey that was pressed.</param>
public class HotkeyPressedEventArgs(HotkeyInfo hotkeyInfo) : EventArgs
{
    /// <summary>
    /// Gets the information about the hotkey that was pressed.
    /// </summary>
    public HotkeyInfo HotkeyInfo { get; } = hotkeyInfo;
}
