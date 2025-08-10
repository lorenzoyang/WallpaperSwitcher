namespace WallpaperSwitcher.Core.Exceptions;

/// <summary>
/// Represents an error that occurs when a hotkey string cannot be parsed correctly.
/// </summary>
public class HotkeyParsingException : Exception
{
    /// <summary>
    /// Gets the hotkey string that caused the parsing failure.
    /// </summary>
    public string InvalidHotkeyString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyParsingException"/> class
    /// with a specified error message and the invalid hotkey string.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="invalidHotkeyString">The hotkey string that could not be parsed.</param>
    public HotkeyParsingException(string message, string invalidHotkeyString) : base(message)
    {
        InvalidHotkeyString = invalidHotkeyString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyParsingException"/> class
    /// with a specified error message, a reference to the inner exception that is
    /// the cause of this exception, and the invalid hotkey string.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that caused the current exception.</param>
    /// <param name="invalidHotkeyString">The hotkey string that could not be parsed.</param>
    public HotkeyParsingException(string message, Exception inner, string invalidHotkeyString) : base(message, inner)
    {
        InvalidHotkeyString = invalidHotkeyString;
    }
}