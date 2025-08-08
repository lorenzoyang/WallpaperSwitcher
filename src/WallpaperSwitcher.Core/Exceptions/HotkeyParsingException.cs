namespace WallpaperSwitcher.Core.GlobalHotKey.Exceptions;

/// <summary>
/// Represents an exception that is thrown when parsing a hotkey string fails due to invalid format or content.
/// </summary>
public class HotkeyParsingException : Exception
{
    /// <summary>
    /// Gets the hotkey text that failed to parse.
    /// </summary>
    public string InvalidHotkeyText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyParsingException"/> class 
    /// with a specified error message and the invalid hotkey text.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="invalidHotkeyText">The hotkey text that failed to be parsed.</param>
    public HotkeyParsingException(string message, string invalidHotkeyText)
        : base(message)
    {
        InvalidHotkeyText = invalidHotkeyText;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyParsingException"/> class 
    /// with a specified error message, a reference to the inner exception that is the cause of this exception,
    /// and the invalid hotkey text.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    /// <param name="invalidHotkeyText">The hotkey text that failed to be parsed.</param>
    public HotkeyParsingException(string message, Exception inner, string invalidHotkeyText)
        : base(message, inner)
    {
        InvalidHotkeyText = invalidHotkeyText;
    }
}