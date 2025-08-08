namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Represents a global hotkey configuration, containing all the information required to
/// register and identify a hotkey combination at the system level.
/// </summary>
/// <remarks>
/// This class encapsulates the full definition of a global hotkey, including its unique ID,
/// the modifier keys, the virtual key, and a human-readable name. Hotkeys defined with this
/// class can trigger system-wide actions, even when the application is not in focus.
/// </remarks>
public class HotkeyInfo : IEquatable<HotkeyInfo>
{
    /// <summary>
    /// Gets the unique identifier for this hotkey registration.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the modifier keys that must be pressed in combination with the main key.
    /// </summary>
    public required ModifierKeys ModifierKeys { get; init; }

    /// <summary>
    /// Gets the primary virtual key that triggers the hotkey when pressed with the specified modifiers.
    /// </summary>
    public required VirtualKeys Key { get; init; }

    /// <summary>
    /// Gets the name associated with this hotkey, typically representing the item or action it is bound to.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Returns a string representation of the hotkey, formatted as a combination of modifier keys and the main key.
    /// </summary>
    /// <returns>The formatted hotkey string.</returns>
    public override string ToString()
    {
        return ModifierKeys == ModifierKeys.None ? Key.ToString() : $"{ModifierKeys.ToFormattedString()}+{Key}";
    }

    /// <summary>
    /// Deconstructs the hotkey information into its component properties.
    /// </summary>
    /// <param name="id">The unique identifier of the hotkey.</param>
    /// <param name="modifierKeys">The modifier keys of the hotkey.</param>
    /// <param name="key">The main virtual key of the hotkey.</param>
    /// <param name="name">The name of the hotkey.</param>
    public void Deconstruct(out int id, out ModifierKeys modifierKeys, out VirtualKeys key, out string name)
    {
        id = Id;
        modifierKeys = ModifierKeys;
        key = Key;
        name = Name;
    }

    #region Equality Members

    public bool Equals(HotkeyInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ModifierKeys == other.ModifierKeys && Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HotkeyInfo);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ModifierKeys, Key);
    }

    public static bool operator ==(HotkeyInfo? left, HotkeyInfo? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(HotkeyInfo? left, HotkeyInfo? right)
    {
        return !Equals(left, right);
    }

    #endregion
}