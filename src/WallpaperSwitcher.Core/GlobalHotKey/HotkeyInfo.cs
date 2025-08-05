namespace WallpaperSwitcher.Core.GlobalHotKey;

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
    /// Attempts to parse a hotkey string into its corresponding modifier and virtual key components.
    /// </summary>
    /// <param name="hotkeyText">The hotkey string to parse (e.g., "Ctrl+Alt+K").</param>
    /// <param name="modifierKeys">
    /// When this method returns, contains the parsed modifier keys if successful; otherwise, <see cref="ModifierKeys.None"/>.
    /// </param>
    /// <param name="virtualKey">
    /// When this method returns, contains the parsed virtual key if successful; otherwise, <see cref="VirtualKeys.None"/>.
    /// </param>
    /// <param name="errorMessage">
    /// When this method returns, contains an error message if the parsing fails; otherwise, an empty string.
    /// </param>
    /// <param name="separator">The separator used to split the hotkey string. Defaults to "+".</param>
    /// <returns><c>true</c> if the hotkey string was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool TryParseFromString(string hotkeyText, out ModifierKeys modifierKeys, out VirtualKeys virtualKey,
        out string errorMessage, string separator = "+")
    {
        modifierKeys = ModifierKeys.None;
        virtualKey = VirtualKeys.None;
        errorMessage = string.Empty;

        var parts = hotkeyText.Split(separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            errorMessage = "Hotkey string must contain at least one modifier and one key.";
            return false;
        }

        // Process all parts except the last one (which should be the main key)
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i].ToUpperInvariant();

            if (i == parts.Length - 1)
            {
                // Last part is the main key
                if (Enum.TryParse<VirtualKeys>(part, out var parsedKey))
                {
                    virtualKey = parsedKey;
                }
                else
                {
                    errorMessage = $"Invalid key: {part}";
                    return false;
                }
            }
            else
            {
                // Previous parts are modifiers
                if (Enum.TryParse<ModifierKeys>(part, ignoreCase: true, out var parsedModifier))
                {
                    modifierKeys |= parsedModifier;
                }
                else
                {
                    errorMessage = $"Invalid modifier: {part}";
                    return false;
                }
            }
        }

        return true;
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