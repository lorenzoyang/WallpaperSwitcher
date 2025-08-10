namespace WallpaperSwitcher.Core.Exceptions;

/// <summary>
/// Represents errors that occur when binding a global hotkey fails.
/// </summary>
public class HotkeyBindingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyBindingException"/> class.
    /// </summary>
    public HotkeyBindingException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyBindingException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public HotkeyBindingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyBindingException"/> class with a specified 
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public HotkeyBindingException(string message, Exception inner)
        : base(message, inner)
    {
    }
}