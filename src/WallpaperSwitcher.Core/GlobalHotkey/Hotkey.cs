namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Represents a keyboard hotkey combination consisting of one or more modifier keys
/// and a primary virtual key.
/// </summary>
/// <param name="ModifierKeys">The modifier keys (e.g., Ctrl, Alt, Shift) used in the hotkey.</param>
/// <param name="VirtualKeys">The primary virtual key (e.g., A, F1) used in the hotkey.</param>
public readonly record struct Hotkey(ModifierKeys ModifierKeys, VirtualKeys VirtualKeys)
{
    /// <summary>
    /// Returns a string representation of the hotkey.
    /// </summary>
    /// <remarks>
    /// If no modifier keys are present, only the primary key is returned.
    /// Otherwise, the result is formatted as <c>Modifier+Key</c>.
    /// </remarks>
    /// <returns>
    /// A string that represents the current hotkey combination.
    /// </returns>
    public override string ToString()
    {
        return ModifierKeys == ModifierKeys.None
            ? VirtualKeys.ToString()
            : $"{ModifierKeys.ToFormattedString()}+{VirtualKeys}";
    }

    /// <summary>
    /// Attempts to parse a string representation of a hotkey into a <see cref="Hotkey"/> instance.
    /// </summary>
    /// <param name="hotkeyString">
    /// The string to parse, containing one or more modifiers and a primary key, 
    /// separated by the specified <paramref name="separator"/>.
    /// </param>
    /// <param name="hotkey">
    /// When this method returns, contains the parsed <see cref="Hotkey"/> if parsing was successful;
    /// otherwise, the default value.
    /// </param>
    /// <param name="errorMessage">
    /// When this method returns, contains an error message describing why parsing failed,
    /// or an empty string if parsing was successful.
    /// </param>
    /// <param name="separator">
    /// The string used to separate modifier keys and the primary key in <paramref name="hotkeyString"/>.
    /// Defaults to <c>"+"</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParseFrom(string hotkeyString, out Hotkey hotkey, out string errorMessage,
        string separator = "+")
    {
        var modifierKeys = ModifierKeys.None;
        var virtualKey = VirtualKeys.None;
        hotkey = default;
        errorMessage = string.Empty;

        var parts = hotkeyString.Split(separator,
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

        hotkey = new Hotkey(modifierKeys, virtualKey);
        return true;
    }
}