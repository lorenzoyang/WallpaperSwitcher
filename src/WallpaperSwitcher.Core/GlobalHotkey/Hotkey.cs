namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Represents a keyboard hotkey combination consisting of one or more modifier keys
/// and a primary virtual key.
/// </summary>
/// <param name="ModifierKeys">The modifier keys (e.g., Ctrl, Alt, Shift) used in the hotkey.</param>
/// <param name="VirtualKeys">The primary virtual key (e.g., A, F1) used in the hotkey.</param>
public readonly record struct Hotkey(ModifierKeys ModifierKeys, VirtualKeys VirtualKeys)
{
    private static readonly IReadOnlyDictionary<string, ModifierKeys> ModifierAliases =
        new Dictionary<string, ModifierKeys>(StringComparer.OrdinalIgnoreCase)
        {
            ["Alt"] = ModifierKeys.Alt,
            ["Ctrl"] = ModifierKeys.Ctrl,
            ["Control"] = ModifierKeys.Ctrl,
            ["Shift"] = ModifierKeys.Shift,
            ["Win"] = ModifierKeys.Win,
            ["Windows"] = ModifierKeys.Win
        };

    /// <summary>
    /// Returns a string representation of the hotkey.
    /// </summary>
    /// <remarks>
    /// Modifier keys are formatted in canonical order before the primary key.
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
    /// The string to parse, containing one or more modifiers and one primary key,
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
    /// A valid global hotkey requires at least one modifier and a non-<c>None</c> key.
    /// </returns>
    public static bool TryParseFrom(string hotkeyString, out Hotkey hotkey, out string errorMessage,
        string separator = "+")
    {
        var modifierKeys = ModifierKeys.None;
        hotkey = default;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(hotkeyString))
        {
            errorMessage = "Hotkey string cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(separator))
        {
            errorMessage = "Hotkey separator cannot be empty.";
            return false;
        }

        var parts = hotkeyString.Split(separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            errorMessage = "Hotkey string must contain at least one modifier and one key.";
            return false;
        }

        for (var i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            if (!ModifierAliases.TryGetValue(part, out var parsedModifier))
            {
                errorMessage = $"Invalid modifier: {part}";
                return false;
            }

            if ((modifierKeys & parsedModifier) == parsedModifier)
            {
                errorMessage = $"Duplicate modifier: {part}";
                return false;
            }

            modifierKeys |= parsedModifier;
        }

        var keyPart = parts[^1];
        if (IsNumericToken(keyPart) ||
            !Enum.TryParse<VirtualKeys>(keyPart, ignoreCase: true, out var virtualKey) ||
            virtualKey == VirtualKeys.None ||
            !Enum.IsDefined(virtualKey))
        {
            errorMessage = $"Invalid key: {keyPart}";
            return false;
        }

        hotkey = new Hotkey(modifierKeys, virtualKey);
        return true;
    }

    private static bool IsNumericToken(string value)
    {
        return uint.TryParse(value, out _);
    }
}
