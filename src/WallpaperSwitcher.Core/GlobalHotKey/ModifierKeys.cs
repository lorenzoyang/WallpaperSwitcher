namespace WallpaperSwitcher.Core.GlobalHotKey;

/// <summary>
/// Specifies the modifier keys that can be combined with virtual keys to create hotkey combinations.
/// This enumeration supports bitwise operations to allow multiple modifiers to be combined.
/// </summary>
/// <remarks>
/// Modifier keys are used in conjunction with regular keys to create hotkey combinations.
/// Multiple modifiers can be combined using bitwise OR operations (e.g., Control | Alt).
/// The values correspond to Windows API modifier key constants used in hotkey registration.
/// </remarks>
[Flags]
public enum ModifierKeys : uint
{
    /// <summary>
    /// No modifier key is pressed. Used when registering hotkeys that consist of only a virtual key
    /// without any modifier keys.
    /// </summary>
    None = 0,

    /// <summary>
    /// The ALT key modifier. When combined with other keys, creates Alt+Key combinations
    /// (e.g., Alt+F4, Alt+Tab).
    /// </summary>
    Alt = 0x0001,

    /// <summary>
    /// The CTRL (Control) key modifier. When combined with other keys, creates Ctrl+Key combinations
    /// (e.g., Ctrl+C, Ctrl+V, Ctrl+Alt+Delete).
    /// </summary>
    Control = 0x0002,

    /// <summary>
    /// The SHIFT key modifier. When combined with other keys, creates Shift+Key combinations
    /// (e.g., Shift+F10, Ctrl+Shift+Esc).
    /// </summary>
    Shift = 0x0004,

    /// <summary>
    /// The Windows logo key modifier. When combined with other keys, creates Win+Key combinations
    /// (e.g., Win+R, Win+L, Win+D). Also known as the "Super" key on some keyboards.
    /// </summary>
    Win = 0x0008
}