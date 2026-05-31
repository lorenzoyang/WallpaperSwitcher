namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Describes a hotkey that could not be registered while loading persisted settings.
/// </summary>
/// <param name="HotkeyInfo">The persisted hotkey that failed to load.</param>
/// <param name="ErrorMessage">The registration error for the hotkey.</param>
public sealed record HotkeyLoadFailure(HotkeyInfo HotkeyInfo, string ErrorMessage);
