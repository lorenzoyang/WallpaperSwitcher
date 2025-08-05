namespace WallpaperSwitcher.Core.GlobalHotKey.Exceptions;

/// <summary>
/// Represents an exception that is thrown when attempting to bind a global hotkey that is already in use.
/// </summary>
public class HotkeyDuplicateBindingException : Exception
{
    /// <summary>
    /// Gets the existing hotkey that caused the conflict.
    /// </summary>
    public HotkeyInfo ExistingHotkey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyDuplicateBindingException"/> class 
    /// with a specified error message and the conflicting <see cref="HotkeyInfo"/>.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="existingHotkeyInfo">The hotkey that is already bound and caused the conflict.</param>
    public HotkeyDuplicateBindingException(string message, HotkeyInfo existingHotkeyInfo)
        : base(message)
    {
        ExistingHotkey = existingHotkeyInfo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyDuplicateBindingException"/> class with a specified 
    /// error message, a reference to the inner exception that is the cause of this exception, 
    /// and the conflicting <see cref="HotkeyInfo"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    /// <param name="existingHotkeyInfo">The hotkey that is already bound and caused the conflict.</param>
    public HotkeyDuplicateBindingException(string message, Exception inner, HotkeyInfo existingHotkeyInfo)
        : base(message, inner)
    {
        ExistingHotkey = existingHotkeyInfo;
    }
}