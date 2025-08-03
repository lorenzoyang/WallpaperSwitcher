namespace WallpaperSwitcher.Core.GlobalHotKey;

/// <summary>
/// Represents a global hotkey configuration containing all the information needed to register
/// and identify a hotkey combination in the system.
/// </summary>
/// <remarks>
/// This class encapsulates the complete definition of a global hotkey, including its unique identifier,
/// the key combination (modifier keys + virtual key), and a human-readable name for identification.
/// Global hotkeys registered with this information will trigger system-wide, regardless of which
/// application currently has focus.
/// </remarks>
public class HotkeyInfo
{
    /// <summary>
    /// Gets the unique identifier for this hotkey registration.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the modifier keys that must be pressed in combination with the main key.
    /// </summary>
    public required ModifierKeys Modifiers { get; init; }

    /// <summary>
    /// Gets the primary virtual key that triggers the hotkey when pressed with the specified modifiers.
    /// </summary>
    public required VirtualKeys Key { get; init; }

    /// <summary>
    /// Gets the human-readable name or description for this hotkey.
    /// </summary>
    public required string Description { get; init; }

    public override string ToString() => Description;
}